using System.Security.Claims;
using LimpidusMongoDB.Application.Auth;
using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Services.Interfaces;

namespace LimpidusMongoDB.Application.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly ISqlAuthDataAccess _sqlAuth;

        public UserManagementService(ISqlAuthDataAccess sqlAuth)
        {
            _sqlAuth = sqlAuth;
        }

        public async Task<Result> ListFranqueadosAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var users = await _sqlAuth.ListFranqueadosAsync(cancellationToken);
                var data = users.Select(MapUser).ToList();

                return Result.Ok(data: data);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Error(ex.Message);
            }
            catch (Exception)
            {
                return Result.Error("Falha ao listar usuários.");
            }
        }

        public async Task<Result> SetAdminAsync(ClaimsPrincipal actor, int franqId, bool isAdmin, CancellationToken cancellationToken = default)
        {
            if (franqId <= 0)
                return Result.Error("Identificador de franqueado inválido.");

            try
            {
                var actorFranqId = GetFranqId(actor);
                if (!isAdmin && actorFranqId.HasValue && actorFranqId.Value == franqId)
                    return Result.Error("Você não pode remover o próprio acesso de Admin.");

                if (!await _sqlAuth.FranqueadoExistsAsync(franqId, cancellationToken))
                    return Result.Error("Franqueado não encontrado ou inativo.");

                await _sqlAuth.SetAdminAsync(franqId, isAdmin, cancellationToken);

                // Recarrega o usuário para devolver grupo/nível atualizados.
                var all = await _sqlAuth.ListFranqueadosAsync(cancellationToken);
                var refreshed = all.FirstOrDefault(u => u.Id == franqId);
                var updated = refreshed != null
                    ? MapUser(refreshed)
                    : new FranqueadoUserResponse
                    {
                        Id = franqId,
                        IsAdmin = isAdmin,
                        IsFranqueado = true,
                        Role = isAdmin ? AuthRoles.Admin : AuthRoles.Franqueado
                    };

                if (refreshed == null)
                {
                    updated.IsAdmin = isAdmin;
                    updated.Role = isAdmin ? AuthRoles.Admin : AuthRoles.Franqueado;
                }

                return Result.Ok(
                    message: isAdmin
                        ? "Usuário promovido a Admin (grupo 1)."
                        : "Acesso Admin removido (grupo 1).",
                    data: updated);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Error(ex.Message);
            }
            catch (Exception)
            {
                return Result.Error("Falha ao atualizar permissão Admin.");
            }
        }

        private static FranqueadoUserResponse MapUser(FranqueadoUserEntity u)
        {
            var isConsultor = !u.IsAdmin && u.HasChildren;
            var role = u.IsAdmin
                ? AuthRoles.Admin
                : isConsultor
                    ? AuthRoles.Consultor
                    : AuthRoles.Franqueado;

            return new FranqueadoUserResponse
            {
                Id = u.Id,
                Nome = u.Nome,
                Login = u.Login,
                IsAdmin = u.IsAdmin,
                IsFranqueado = true,
                IsConsultor = isConsultor,
                Role = role,
                NivelId = u.NivelId,
                NivelNome = u.NivelNome,
                NivelGrupoId = u.NivelGrupoId,
                Grupos = u.Grupos
            };
        }

        private static int? GetFranqId(ClaimsPrincipal user)
        {
            var raw = user?.FindFirst(AuthClaims.FranqId)?.Value;
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
