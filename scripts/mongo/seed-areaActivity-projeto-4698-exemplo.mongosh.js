'''''''''''''''''/**
 * “INSERT” em lote na coleção areaActivity (MongoDB não usa SQL).
 *
 * Ajuste DB_NAME, EMPLOYEE_ID e a lista AREAS antes de rodar.
 *
 * Uso:
 *   mongosh "<connection-string>" scripts/mongo/seed-areaActivity-projeto-4698-exemplo.mongosh.js
 *
 * Ou no mongosh já conectado:
 *   load("scripts/mongo/seed-areaActivity-projeto-4698-exemplo.mongosh.js")
 *
 * weekDays: 0=Dom … 6=Sáb (igual à API). itemId = string (ex.: legado WORK_TAREFAS).
 */

const DB_NAME = "limpidus"; // AppSettings:Database
const PROJECT_LEGACY_ID = 4698;
/** _id do documento em employee (24 hex) — o mesmo que o app usa no GET de áreas */
const EMPLOYEE_ID = "69fb88221d9063722b3b8af9";

const now = new Date();

/** Frequência “todos os dias” por lista (alternativa: type "daily" na API) */
const freqTodosDias = { type: "weekly", weekDays: [0, 1, 2, 3, 4, 5, 6] };

/**
 * Nomes de exemplo (alinhe com o que tinham no SQL/app). Duplique/edite linhas à vontade.
 */
const AREAS = [
  { name: "SALA REUNIÃO 1", orderBy: 1, totalM2: 40, items: [{ itemId: "1", name: "Esvaziar cesto lixo (sacos)", orderBy: 1, frequency: freqTodosDias }] },
  { name: "SALA REUNIÃO 2", orderBy: 2, totalM2: 0, items: [] },
  { name: "SALA REUNIÃO 3", orderBy: 3, totalM2: 0, items: [] },
  { name: "DML", orderBy: 4, totalM2: 0, items: [] },
  { name: "HALL ELEVADORES / RECEPÇÃO", orderBy: 5, totalM2: 0, items: [] },
  { name: "RECEPÇÃO", orderBy: 6, totalM2: 0, items: [] },
];

// --- validação mínima ---
if (!/^[a-fA-F0-9]{24}$/.test(EMPLOYEE_ID)) {
  print("ERRO: defina EMPLOYEE_ID com um ObjectId válido (24 hex).");
  quit(1);
}

use(DB_NAME);

const emp = db.employee.findOne({ _id: ObjectId(EMPLOYEE_ID) });
if (!emp) {
  print("ERRO: employee não encontrado: " + EMPLOYEE_ID);
  quit(1);
}

const docs = AREAS.map((a) => ({
  _id: new ObjectId(),
  createdDate: now,
  updateDate: now,
  name: a.name,
  description: "",
  quickTask: false,
  totalM2: a.totalM2 || 0,
  employeeId: EMPLOYEE_ID,
  headerId: "",
  orderBy: a.orderBy,
  frequency: null,
  projectId: PROJECT_LEGACY_ID,
  items: (a.items || []).map((it) => ({
    itemId: String(it.itemId),
    name: it.name,
    orderBy: it.orderBy,
    frequency: it.frequency
      ? {
          type: it.frequency.type,
          weekDays: it.frequency.weekDays,
        }
      : null,
  })),
}));

const r = db.areaActivity.insertMany(docs, { ordered: false });
print(`Inseridos: ${Object.keys(r.insertedIds).length} documento(s) em areaActivity (projectId=${PROJECT_LEGACY_ID}).`);
print("IDs gerados (guarde se precisar de PATCH depois):");
printjson(Object.values(r.insertedIds).map((id) => id.toString()));
'''''''''''''''''