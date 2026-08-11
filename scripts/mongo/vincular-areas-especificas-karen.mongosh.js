/**
 * Vincula apenas algumas áreas (areaActivity) à Karen pelo employeeId.
 *
 * Áreas (campo "name" do documento): Controler, Cabine 2, Cabine 1
 * Employee Karen: _id em string abaixo
 *
 * Uso:
 *   mongosh "<connection-string>" scripts/mongo/vincular-areas-especificas-karen.mongosh.js
 */

const DB_NAME = "limpidus";
const PROJECT_LEGACY_ID = 4698; // legacyId do projeto (projectId em areaActivity)
const KAREN_EMPLOYEE_ID = "69695cf9bd40858434323978";

/** Nomes exatos como estão no Mongo (campo name). Ajuste se no banco estiver diferente. */
const AREA_NAMES = ["Controler", "Cabine 2", "Cabine 1"];

use(DB_NAME);

const filtro = {
  projectId: PROJECT_LEGACY_ID,
  name: { $in: AREA_NAMES },
};

print("Documentos que serão atualizados:");
db.areaActivity.find(filtro, { name: 1, employeeId: 1, projectId: 1 }).forEach(printjson);

const karen = db.employee.findOne(
  { _id: ObjectId(KAREN_EMPLOYEE_ID) },
  { firstName: 1, lastName: 1, number: 1 }
);
if (!karen) {
  print(`Employee não encontrado: ${KAREN_EMPLOYEE_ID}`);
  quit(1);
}
print("Funcionário alvo (Karen):");
printjson(karen);

const resultado = db.areaActivity.updateMany(filtro, {
  $set: { employeeId: KAREN_EMPLOYEE_ID },
});

print("Resultado updateMany:");
printjson({
  matchedCount: resultado.matchedCount,
  modifiedCount: resultado.modifiedCount,
});
