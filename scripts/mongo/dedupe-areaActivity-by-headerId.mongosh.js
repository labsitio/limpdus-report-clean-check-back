git /**
 * Remove documentos duplicados em `areaActivity` com o mesmo
 * `projectId` + `employeeId` + `headerId` (headerId não vazio),
 * mantendo o que tem mais itens em `items` (empate: maior `_id` em hex).
 *
 * Causa típica: vários POST /v1/AreaActivity com `id` vazio para a mesma área,
 * ou migração repetida — o app lista várias vezes "DML" e o QR pode apontar
 * para um documento sem tarefas.
 *
 * Uso:
 *   mongosh "<connection-string>" dedupe-areaActivity-by-headerId.mongosh.js
 *
 * Revise DB_NAME e PROJECT_LEGACY_ID antes de executar.
 */

const DB_NAME = "limpidus";
const PROJECT_LEGACY_ID = 4698;

use(DB_NAME);

const cursor = db.areaActivity.find({
  projectId: PROJECT_LEGACY_ID,
  headerId: { $exists: true, $nin: [null, ""] },
});

const groups = new Map();

cursor.forEach((doc) => {
  const emp = doc.employeeId ? String(doc.employeeId) : "";
  const hid = String(doc.headerId).trim();
  const key = `${emp}|${hid}`;
  const n = Array.isArray(doc.items) ? doc.items.length : 0;
  if (!groups.has(key)) groups.set(key, []);
  groups.get(key).push({ _id: doc._id, n, name: doc.name });
});

let deleted = 0;
for (const [, docs] of groups) {
  if (docs.length <= 1) continue;
  docs.sort((a, b) => {
    if (b.n !== a.n) return b.n - a.n;
    return String(b._id).localeCompare(String(a._id));
  });
  const keep = docs[0];
  const remove = docs.slice(1);
  print(
    `Grupo ${docs[0].name} (${docs.length} docs): manter _id=${keep._id} (items=${keep.n}), apagar ${remove.length}`,
  );
  remove.forEach((d) => {
    db.areaActivity.deleteOne({ _id: d._id });
    deleted++;
  });
}

print(`Concluído. Documentos apagados: ${deleted}`);
