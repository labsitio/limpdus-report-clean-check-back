using LimpidusMongoDB.Application.Auth;
using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;
using LimpidusMongoDB.Application.Helpers;
using LimpidusMongoDB.Application.Services.Interfaces;

namespace LimpidusMongoDB.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ISqlAuthDataAccess _sqlAuth;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IProjectRepository _projectRepository;

        public AuthService(
            ISqlAuthDataAccess sqlAuth,
            IJwtTokenService jwtTokenService,
            IProjectRepository projectRepository)
        {
            _sqlAuth = sqlAuth;
            _jwtTokenService = jwtTokenService;
            _projectRepository = projectRepository;
        }

        public async Task<Result> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
                return Result.Error("Login e senha são obrigatórios.");

            var type = (request.Type ?? AuthLoginTypes.Auto).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(type))
                type = AuthLoginTypes.Auto;

            try
            {
                return type switch
                {
                    AuthLoginTypes.Auto => await LoginAutoAsync(request.Login.Trim(), request.Password, cancellationToken),
                    AuthLoginTypes.Franqueado => await LoginFranqueadoAsync(request.Login.Trim(), request.Password, cancellationToken),
                    AuthLoginTypes.Project => await LoginProjectAsync(request.Login.Trim(), request.Password, cancellationToken),
                    _ => Result.Error("Tipo de login invalido. Use auto, franqueado ou project.")
                };
            }
            catch (InvalidOperationException ex)
            {
                return Result.Error(ex.Message);
            }
            catch (Exception)
            {
                return Result.Error("Falha ao autenticar. Verifique a conexão SQL / configuração.");
            }
        }


        private async Task<Result> LoginAutoAsync(string login, string password, CancellationToken cancellationToken)
        {
            var franqueado = await LoginFranqueadoAsync(login, password, cancellationToken);
            if (franqueado.Success)
                return franqueado;

            return await LoginProjectAsync(login, password, cancellationToken);
        }

        private async Task<Result> LoginFranqueadoAsync(string login, string password, CancellationToken cancellationToken)
        {
            var hash = Md5Hasher.HashHex(password);
            var franqueado = await _sqlAuth.ValidateFranqueadoAsync(login, hash, cancellationToken);
            if (franqueado == null)
                return Result.Error("Usuário ou senha inválidos.");

            var isAdmin = await _sqlAuth.IsAdminAsync(franqueado.Id, cancellationToken);

            string role;
            IReadOnlyList<AllowedProjectResponse> projects;
            bool bypassAllowedProjects;

            if (isAdmin)
            {
                // Admin: todos os projetos migrados (Mongo), bypass por role.
                role = AuthRoles.Admin;
                projects = await GetAdminProjectsAsync(cancellationToken);
                bypassAllowedProjects = true;
            }
            else
            {
                var hierarchy = await _sqlAuth.GetHierarchyContextAsync(franqueado.Id, cancellationToken);
                if (hierarchy.IsConsultor)
                {
                    // Consultor: carteira hierárquica (nós abaixo + VER_NIVEL), relatório completo.
                    role = AuthRoles.Consultor;
                    projects = await IntersectWithMongoIfAvailableAsync(
                        await _sqlAuth.GetConsultorProjectsAsync(franqueado.Id, cancellationToken),
                        cancellationToken);
                }
                else
                {
                    // Franqueado folha: só ID_DONO + WORK_HEADER_SHARE.
                    role = AuthRoles.Franqueado;
                    projects = await IntersectWithMongoIfAvailableAsync(
                        await _sqlAuth.GetFranqueadoProjectsAsync(franqueado.Id, cancellationToken),
                        cancellationToken);
                }

                bypassAllowedProjects = false;
            }

            var primary = PickPrimaryProject(projects);

            var (token, expires) = _jwtTokenService.CreateToken(new AuthTokenPayload
            {
                Role = role,
                IsFranqueado = true,
                FranqId = franqueado.Id,
                LegacyProjectId = primary?.Id,
                DisplayName = franqueado.Nome,
                AllowedProjectIds = bypassAllowedProjects
                    ? Array.Empty<int>()
                    : projects.Select(p => p.Id).ToArray()
            });

            return Result.Ok(data: new LoginResponse
            {
                Token = token,
                Role = role,
                IsFranqueado = true,
                IsAdmin = isAdmin,
                FranqId = franqueado.Id,
                IdProjeto = primary?.Id ?? 0,
                Nome = primary?.Name ?? franqueado.Nome,
                AllowedProjects = projects,
                ExpiresAtUtc = expires
            });
        }

        /// <summary>
        /// Projetos disponíveis no Clean Check (Mongo). Preferência N3 (level=3 / nome).
        /// Fallback SQL se Mongo estiver vazio.
        /// </summary>
        private async Task<IReadOnlyList<AllowedProjectResponse>> GetAdminProjectsAsync(CancellationToken cancellationToken)
        {
            var mongoProjects = await _projectRepository.FindAllAsync();
            if (mongoProjects != null && mongoProjects.Any())
            {
                return mongoProjects
                    .OrderBy(p => p.Level == 3 ? 0 : 1)
                    .ThenBy(p => p.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .Select(p => new AllowedProjectResponse
                    {
                        Id = p.LegacyId,
                        Name = p.Name ?? string.Empty
                    })
                    .ToList();
            }

            return await _sqlAuth.GetAllProjectsAsync(cancellationToken);
        }

        /// <summary>
        /// Mantém só projetos que existem no Mongo (como a lista do Admin), se houver dados migrados.
        /// </summary>
        private async Task<IReadOnlyList<AllowedProjectResponse>> IntersectWithMongoIfAvailableAsync(
            IReadOnlyList<AllowedProjectResponse> sqlProjects,
            CancellationToken cancellationToken)
        {
            if (sqlProjects == null || sqlProjects.Count == 0)
                return sqlProjects ?? Array.Empty<AllowedProjectResponse>();

            var mongoProjects = await _projectRepository.FindAllAsync();
            if (mongoProjects == null || !mongoProjects.Any())
                return sqlProjects;

            var mongoIds = mongoProjects.Select(p => p.LegacyId).ToHashSet();
            return sqlProjects
                .Where(p => mongoIds.Contains(p.Id))
                .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Preferência: projeto N3 explícito no nome (ex. Cardoso CC N3), senão o primeiro da lista
        /// (Admin já ordena level=3 primeiro).
        /// </summary>
        private static AllowedProjectResponse? PickPrimaryProject(IReadOnlyList<AllowedProjectResponse> projects)
        {
            if (projects == null || projects.Count == 0)
                return null;

            var n3ByName = projects.FirstOrDefault(p =>
                !string.IsNullOrEmpty(p.Name) &&
                p.Name.IndexOf("N3", StringComparison.OrdinalIgnoreCase) >= 0);
            return n3ByName ?? projects[0];
        }

        private async Task<Result> LoginProjectAsync(string login, string password, CancellationToken cancellationToken)
        {
            var project = await _sqlAuth.ValidateProjectLoginAsync(login, password, cancellationToken);
            if (project == null)
                return Result.Error("Usuário ou senha inválidos.");

            var allowed = new[]
            {
                new AllowedProjectResponse { Id = project.WorkHeaderId, Name = project.NomeProjeto }
            };

            var (token, expires) = _jwtTokenService.CreateToken(new AuthTokenPayload
            {
                Role = AuthRoles.ProjectViewer,
                IsFranqueado = false,
                FranqId = null,
                LegacyProjectId = project.WorkHeaderId,
                DisplayName = project.NomeProjeto,
                AllowedProjectIds = new[] { project.WorkHeaderId }
            });

            return Result.Ok(data: new LoginResponse
            {
                Token = token,
                Role = AuthRoles.ProjectViewer,
                IsFranqueado = false,
                IsAdmin = false,
                FranqId = null,
                IdProjeto = project.WorkHeaderId,
                Nome = project.NomeProjeto,
                AllowedProjects = allowed,
                ExpiresAtUtc = expires
            });
        }
    }
}
