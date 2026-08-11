/**
 * Remove registos duplicados na coleção `history` (mesmo envio repetido / double-tap).
 *
 * Chave de duplicado (alinhada à lógica de HistoryService.SaveAsync):
 *   projectId + employeeId + areaTaskId
 *   + endDate truncada ao segundo (UTC)
 *   + impressão digital da justificativa (information + reason)
 *   + impressão digital dos items (id + performed + orderBy), ordem independente
 *
 * Em cada grupo mantém o documento com menor _id (inserção mais antiga) e apaga os restantes.
 * Opcionalmente remove linhas em `itemHistory` com historyId = _id apagado (se existirem).
 *
 * Uso:
 *   mongosh "<connection-string>" dedupe-history.mongosh.js
 *
 * 1) Deixe DRY_RUN = true, confira o print.
 * 2) DRY_RUN = false e, se quiser, defina PROJECT_LEGACY_ID (número) ou null para todos os projetos.
 */

const DB_NAME = "limpidus";
/** @type {number|null} null = todos os projectId */
const PROJECT_LEGACY_ID = null;
const DRY_RUN = true;
/** Se true, apaga também itemHistory cujo historyId seja um dos _id removidos */
const CLEAN_ITEM_HISTORY = true;

const SEP = "\u001f";

function readJustification(j) {
  if (!j) return SEP;
  const inf = j.information != null ? j.information : j.Information;
  const rea = j.reason != null ? j.reason : j.Reason;
  return `${inf ?? ""}${SEP}${rea ?? ""}`;
}

function readItemFingerprint(items) {
  if (!items || !items.length) return "";
  const parts = [];
  for (const i of items) {
    const id = i.id != null ? i.id : i.Id;
    const performed = !!(i.performed != null ? i.performed : i.Performed);
    const ob = i.orderBy != null ? i.orderBy : i.OrderBy;
    parts.push(`${id ?? ""}${SEP}${performed ? "1" : "0"}${SEP}${ob ?? ""}`);
  }
  parts.sort();
  return parts.join("|");
}

function docDedupeKey(doc) {
  const pid = doc.projectId != null ? doc.projectId : doc.ProjectId;
  const emp = String(doc.employeeId != null ? doc.employeeId : doc.EmployeeId ?? "");
  const at = String(doc.areaTaskId != null ? doc.areaTaskId : doc.AreaTaskId ?? "");
  const ed = doc.endDate != null ? doc.endDate : doc.EndDate;
  const sec = ed ? Math.floor(new Date(ed).getTime() / 1000) : 0;
  const items = doc.items != null ? doc.items : doc.Items;
  const just = doc.justification != null ? doc.justification : doc.Justification;
  return `${pid}|${emp}|${at}|${sec}|${readJustification(just)}|${readItemFingerprint(items)}`;
}

use(DB_NAME);

const match = {};
if (PROJECT_LEGACY_ID != null) match.projectId = PROJECT_LEGACY_ID;

const byKey = new Map();
db.history.find(match).forEach((doc) => {
  const k = docDedupeKey(doc);
  if (!byKey.has(k)) byKey.set(k, []);
  byKey.get(k).push(doc);
});

let deletedHistory = 0;
let deletedItemHistory = 0;

for (const [, docs] of byKey) {
  if (docs.length <= 1) continue;
  docs.sort((a, b) => String(a._id).localeCompare(String(b._id)));
  const keep = docs[0];
  const remove = docs.slice(1);
  print(
    `Grupo ${docs.length} dup — manter _id=${keep._id}, remover ${remove.length} (área=${keep.areaTaskName ?? keep.AreaTaskName ?? "?"})`,
  );
  for (const d of remove) {
    const hid = String(d._id);
    if (CLEAN_ITEM_HISTORY) {
      if (DRY_RUN) {
        deletedItemHistory += db.itemHistory.countDocuments({ historyId: hid });
      } else {
        const r = db.itemHistory.deleteMany({ historyId: hid });
        deletedItemHistory += r.deletedCount ?? 0;
      }
    }
    if (!DRY_RUN) {
      db.history.deleteOne({ _id: d._id });
    }
    deletedHistory++;
  }
}

if (DRY_RUN) {
  print(
    `\nDRY_RUN=true — Nada foi apagado. Seriam removidos: ${deletedHistory} documentos em history` +
      (CLEAN_ITEM_HISTORY ? ` e ${deletedItemHistory} referências em itemHistory (count).` : "."),
  );
  print("Altere DRY_RUN para false e volte a executar para aplicar.");
} else {
  print(`\nConcluído. Removidos ${deletedHistory} documentos em history; itemHistory linhas apagadas: ${deletedItemHistory}.`);
}
