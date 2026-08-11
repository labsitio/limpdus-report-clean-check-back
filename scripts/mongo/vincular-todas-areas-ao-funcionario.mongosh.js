/**
 * Vincula TODAS as áreas (coleção areaActivity) de um projeto legado
 * a um único funcionário (employeeId = _id do employee em string).
 *
 * Uso (mongosh):
 *   mongosh "<sua-connection-string>" vincular-todas-areas-ao-funcionario.mongosh.js
 *
 * Ou dentro do mongosh já conectado:
 *   load("scripts/mongo/vincular-todas-areas-ao-funcionario.mongosh.js")
 *
 * ATENÇÃO: isso sobrescreve employeeId em TODOS os documentos areaActivity
 * com o mesmo projectId (legado). Revise antes de rodar em produção.
 */

// --- ajuste se necessário ---
const DB_NAME = "limpidus"; // mesmo valor de AppSettings:Database
const PROJECT_LEGACY_ID = 4698; // legacyId do projeto (campo projectId em areaActivity)
const TARGET_EMPLOYEE_ID = "69fb88221d9063722b3b8af9"; // _id do employee (24 hex, string)

use(DB_NAME);

const filtro = { projectId: PROJECT_LEGACY_ID };

const antes = db.areaActivity.countDocuments(filtro);
print(`Documentos areaActivity com projectId=${PROJECT_LEGACY_ID}: ${antes}`);

if (antes === 0) {
  print("Nada a atualizar (0 documentos). Confira DB_NAME e PROJECT_LEGACY_ID.");
  quit(1);
}

const funcionario = db.employee.findOne(
  { _id: ObjectId(TARGET_EMPLOYEE_ID) },
  { firstName: 1, lastName: 1, number: 1, projectId: 1 }
);
if (!funcionario) {
  print(`Employee não encontrado: ${TARGET_EMPLOYEE_ID}`);
  quit(1);
}
print("Funcionário alvo:");
printjson(funcionario);

const resultado = db.areaActivity.updateMany(filtro, {
  $set: { employeeId: TARGET_EMPLOYEE_ID },
});

print("Resultado updateMany:");
printjson({
  matchedCount: resultado.matchedCount,
  modifiedCount: resultado.modifiedCount,
});

print("Pronto. Todas as áreas desse projectId foram vinculadas ao employeeId informado.");
