/**
 * Apaga documentos em `history` sem lista de tarefas (items ausente, null ou array vazio).
 * Corresponde a linhas no app só com o nome da área (ex.: "DML", "WC Masc 1") sem subtarefas.
 *
 * Uso:
 *   mongosh "<connection-string>" delete-history-without-items.mongosh.js
 *
 * 1) DRY_RUN = true — só conta e mostra exemplos.
 * 2) DRY_RUN = false — apaga (faça backup antes em produção).
 */

const DB_NAME = "limpidus";
/** @type {number|null} null = todos os projectId */
const PROJECT_LEGACY_ID = null;
const DRY_RUN = true;
const CLEAN_ITEM_HISTORY = true;
const MAX_SAMPLE = 40;

/** Documentos sem items úteis para o histórico de conclusão */
const filterNoItems = {
  $or: [
    { items: { $exists: false } },
    { items: null },
    { items: { $size: 0 } },
  ],
};

const filter =
  PROJECT_LEGACY_ID != null
    ? { $and: [{ projectId: PROJECT_LEGACY_ID }, filterNoItems] }
    : filterNoItems;

use(DB_NAME);

const total = db.history.countDocuments(filter);
print(`Documentos history sem items (filtro): ${total}`);

let shown = 0;
db.history.find(filter).forEach((doc) => {
  if (shown < MAX_SAMPLE) {
    printjson({
      _id: doc._id,
      projectId: doc.projectId,
      areaTaskName: doc.areaTaskName ?? doc.AreaTaskName,
      areaTaskId: doc.areaTaskId ?? doc.AreaTaskId,
      endDate: doc.endDate ?? doc.EndDate,
      createdDate: doc.createdDate ?? doc.CreatedDate,
    });
    shown++;
  }
});
if (total > MAX_SAMPLE) {
  print(`... (+${total - MAX_SAMPLE} não listados)`);
}

if (DRY_RUN) {
  print(
    "\nDRY_RUN=true — nada apagado. Para aplicar: DRY_RUN=false e volte a executar.",
  );
  quit(0);
}

let itemHistDeleted = 0;
const ids = db.history.find(filter, { _id: 1 }).toArray();
for (const d of ids) {
  const hid = String(d._id);
  if (CLEAN_ITEM_HISTORY) {
    const r = db.itemHistory.deleteMany({ historyId: hid });
    itemHistDeleted += r.deletedCount ?? 0;
  }
}

const del = db.history.deleteMany(filter);
print(`Apagados history: ${del.deletedCount ?? 0}`);
print(`Apagados itemHistory (por historyId): ${itemHistDeleted}`);
