/**
 * Gera examples/api/areaActivity-bulk-project-4698.example.json
 * a partir dos dados legados (mesmas linhas que scripts/sql/generate-json-areaActivity-projeto-4698.sql).
 *
 * Lista de áreas = apenas as que aparecem no app (ecrãs Cardoso N3), nesta ordem.
 * Há 21 linhas na UI e 19 WORK_AREA_ID no extract: "WC MASC 2" e "DEPÓSITO/SERVIDOR" ficam sem
 * headerId/tarefas até mapeares no SQL (ajuste o array areasLayout).
 *
 * node scripts/node/build-areaActivity-bulk-4698-json.mjs
 */

import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const outPath = path.join(__dirname, "../../examples/api/areaActivity-bulk-project-4698.example.json");

const PROJECT_ID = 4698;
const EMPLOYEE_PLACEHOLDER = "COLE_AQUI_OBJECTID_24_HEX_DO_FUNCIONARIO_MONGO";

/** Mesmas linhas que #Tarefas no SQL (Tarefa, Descricao, Periodo, FrequenciaDias) */
const rawTasks = [
  [53683, 7, "Tirar pó de mesas e superficies horizontais.", "RT", 260],
  [53683, 9, "Tirar pó de superficies altas.", "LV", 52],
  [53683, 15, "Remover marcas de paredes, portas e divisórias.", "LV", 52],
  [53683, 27, "Mop pó", "LV", 260],
  [53683, 29, "Mop úmido", "RT", 260],
  [53685, 3, "Levar o lixo coletado", "LV", 260],
  [53685, 9, "Tirar pó de superficies altas.", "LV", 52],
  [53685, 27, "Mop pó", "LV", 260],
  [53685, 29, "Mop úmido", "RT", 260],
  [53685, 179, "Limpeza de refrigerador (*)", "LV", 52],
  [53685, 222, "Esvaziar lixo (cozinha)", "LV", 312],
  [53685, 232, "Limpeza úmida de mesas (*)", "LV", 260],
  [53685, 236, "Limpeza de forno microondas (*)", "LV", 52],
  [53687, 1, "Esvaziar cesto lixo (sacos)", "LV", 260],
  [53687, 3, "Levar o lixo coletado", "LV", 260],
  [53687, 7, "Tirar pó de mesas e superficies horizontais.", "RT", 260],
  [53687, 9, "Tirar pó de superficies altas.", "LV", 52],
  [53687, 13, "Limpar pés de cadeiras", "RT", 26],
  [53687, 17, "Limpeza de telefones", "RT", 52],
  [53687, 22, "Aspirar linhas de tráfego", "RT", 208],
  [53687, 24, "Aspiração completa", "RT", 52],
  [53689, 1, "Esvaziar cesto lixo (sacos)", "LV", 260],
  [53689, 3, "Levar o lixo coletado", "LV", 260],
  [53689, 7, "Tirar pó de mesas e superficies horizontais.", "RT", 260],
  [53689, 9, "Tirar pó de superficies altas.", "LV", 52],
  [53689, 13, "Limpar pés de cadeiras", "RT", 26],
  [53689, 17, "Limpeza de telefones", "RT", 52],
  [53689, 22, "Aspirar linhas de tráfego", "RT", 208],
  [53689, 24, "Aspiração completa", "RT", 52],
  [53695, 1, "Esvaziar cesto lixo (sacos)", "LV", 260],
  [53695, 3, "Levar o lixo coletado", "LV", 260],
  [53695, 7, "Tirar pó de mesas e superficies horizontais.", "RT", 260],
  [53695, 9, "Tirar pó de superficies altas.", "LV", 52],
  [53695, 13, "Limpar pés de cadeiras", "RT", 26],
  [53695, 14, "Tirar poeira alta - detalhada", "LV", 12],
  [53695, 17, "Limpeza de telefones", "RT", 52],
  [53695, 22, "Aspirar linhas de tráfego", "RT", 208],
  [53695, 24, "Aspiração completa", "RT", 52],
  [53695, 86, "Aspirar móveis de escritório(*)", "RT", 4],
  [53697, 1, "Esvaziar cesto lixo (sacos)", "LV", 260],
  [53697, 3, "Levar o lixo coletado", "LV", 260],
  [53697, 7, "Tirar pó de mesas e superficies horizontais.", "RT", 260],
  [53697, 9, "Tirar pó de superficies altas.", "LV", 52],
  [53697, 13, "Limpar pés de cadeiras", "RT", 26],
  [53697, 22, "Aspirar linhas de tráfego", "RT", 208],
  [53697, 24, "Aspiração completa", "RT", 52],
  [53697, 86, "Aspirar móveis de escritório(*)", "RT", 4],
  [63941, 1, "Esvaziar cesto lixo (sacos)", "LV", 260],
  [63941, 3, "Levar o lixo coletado", "LV", 260],
  [63941, 45, "Limpeza completa de WC individual (*)", "RT", 260],
  [63943, 1, "Esvaziar cesto lixo (sacos)", "LV", 260],
  [63943, 3, "Levar o lixo coletado", "LV", 260],
  [63943, 45, "Limpeza completa de WC individual (*)", "RT", 260],
  [63947, 1, "Esvaziar cesto lixo (sacos)", "LV", 260],
  [63947, 3, "Levar o lixo coletado", "LV", 260],
  [63947, 44, "Limpeza completa de WC coletivo (*)", "RT", 260],
  [63949, 1, "Esvaziar cesto lixo (sacos)", "LV", 260],
  [63949, 3, "Levar o lixo coletado", "LV", 260],
  [63949, 45, "Limpeza completa de WC individual (*)", "RT", 260],
  [63951, 1, "Esvaziar cesto lixo (sacos)", "LV", 260],
  [63951, 3, "Levar o lixo coletado", "LV", 260],
  [63951, 7, "Tirar pó de mesas e superficies horizontais.", "RT", 260],
  [63951, 9, "Tirar pó de superficies altas.", "LV", 52],
  [63951, 13, "Limpar pés de cadeiras", "RT", 26],
  [63951, 15, "Remover marcas de paredes, portas e divisórias.", "LV", 52],
  [63951, 17, "Limpeza de telefones", "RT", 52],
  [63951, 22, "Aspirar linhas de tráfego", "RT", 208],
  [63951, 24, "Aspiração completa", "RT", 52],
  [63951, 52, "Limpeza de divisória de vidro", "LV", 26],
  [63951, 232, "Limpeza úmida de mesas (*)", "LV", 0],
  [63953, 1, "Esvaziar cesto lixo (sacos)", "LV", 260],
  [63953, 3, "Levar o lixo coletado", "LV", 260],
  [63953, 7, "Tirar pó de mesas e superficies horizontais.", "RT", 260],
  [63953, 9, "Tirar pó de superficies altas.", "LV", 52],
  [63953, 13, "Limpar pés de cadeiras", "RT", 26],
  [63953, 15, "Remover marcas de paredes, portas e divisórias.", "LV", 52],
  [63953, 17, "Limpeza de telefones", "RT", 52],
  [63953, 22, "Aspirar linhas de tráfego", "RT", 208],
  [63953, 24, "Aspiração completa", "RT", 52],
  [63953, 232, "Limpeza úmida de mesas (*)", "LV", 0],
  [64025, 7, "Tirar pó de mesas e superficies horizontais.", "RT", 260],
  [64025, 27, "Mop pó", "LV", 260],
  [64025, 29, "Mop úmido", "RT", 260],
  [64025, 51, "Limpeza de porta de vidro (*)", "LV", 260],
  [64027, 7, "Tirar pó de mesas e superficies horizontais.", "RT", 260],
  [64027, 9, "Tirar pó de superficies altas.", "LV", 52],
  [64027, 15, "Remover marcas de paredes, portas e divisórias.", "LV", 52],
  [64027, 24, "Aspiração completa", "RT", 52],
  [64029, 7, "Tirar pó de mesas e superficies horizontais.", "RT", 260],
  [64029, 9, "Tirar pó de superficies altas.", "LV", 52],
  [64029, 15, "Remover marcas de paredes, portas e divisórias.", "LV", 52],
  [64029, 24, "Aspiração completa", "RT", 52],
  [64029, 232, "Limpeza úmida de mesas (*)", "LV", 0],
  [64029, 233, "Limpeza úmida de cadeiras (*)", "LV", 260],
  [64031, 1, "Esvaziar cesto lixo (sacos)", "LV", 260],
  [64031, 3, "Levar o lixo coletado", "LV", 260],
  [64031, 7, "Tirar pó de mesas e superficies horizontais.", "RT", 260],
  [64031, 9, "Tirar pó de superficies altas.", "LV", 52],
  [64031, 13, "Limpar pés de cadeiras", "RT", 26],
  [64031, 17, "Limpeza de telefones", "RT", 52],
  [64031, 22, "Aspirar linhas de tráfego", "RT", 208],
  [64031, 24, "Aspiração completa", "RT", 52],
  [64033, 3, "Levar o lixo coletado", "LV", 260],
  [64033, 9, "Tirar pó de superficies altas.", "LV", 52],
  [64033, 19, "Limpeza de pias (*)", "LV", 260],
  [64033, 29, "Mop úmido", "RT", 260],
  [64035, 9, "Tirar pó de superficies altas.", "LV", 52],
  [64035, 11, "Tirar poeira baixa", "LV", 52],
  [64035, 22, "Aspirar linhas de tráfego", "RT", 208],
  [64195, 1, "Esvaziar cesto lixo (sacos)", "LV", 260],
  [64195, 3, "Levar o lixo coletado", "LV", 260],
  [64195, 7, "Tirar pó de mesas e superficies horizontais.", "RT", 260],
  [64195, 9, "Tirar pó de superficies altas.", "LV", 52],
  [64195, 13, "Limpar pés de cadeiras", "RT", 26],
  [64195, 17, "Limpeza de telefones", "RT", 52],
  [64195, 20, "Tirar pó de persianas com retentor", "RT", 52],
  [64195, 22, "Aspirar linhas de tráfego", "RT", 208],
  [64195, 24, "Aspiração completa", "RT", 52],
  [64195, 72, "Tirar poeira de equipamentos", "LV", 260],
];

function freqType(dias) {
  switch (dias) {
    case 1:
      return "yearly";
    case 2:
      return "semi-annual";
    case 4:
      return "quarterly";
    case 6:
      return "bimonthly";
    case 12:
      return "monthly";
    case 26:
      return "biweekly";
    case 52:
    case 260:
    case 208:
    case 312:
      return "weekly";
    case 365:
      return "everyday";
    default:
      return "weekly";
  }
}

function weekDays(dias) {
  if (dias === 312) return [1, 2, 3, 4, 5, 6];
  if (dias === 208) return [1, 2, 3, 4];
  if (dias === 4) return [0, 1, 2, 3, 4, 5, 6];
  if (dias === 12) return [1, 2, 3, 4, 5, 6];
  if (dias === 0) return [1, 2, 3, 4, 5];
  return [1, 2, 3, 4, 5];
}

const seen = new Set();
const tasks = [];
for (const row of rawTasks) {
  const key = `${row[0]}:${row[1]}`;
  if (seen.has(key)) continue;
  seen.add(key);
  tasks.push({
    workAreaId: row[0],
    tarefa: row[1],
    descricao: row[2],
    periodo: row[3],
    dias: row[4],
  });
}

/**
 * Ordem = scroll do app (lista completa). `wid` null = sem tarefas no extract (#Tarefas só tem 19 WORK_AREA_ID).
 * Há 21 linhas na UI: WC MASC 2 + OPERAÇÕES ficam null até existir WORK_AREA_ID+tarefas no legado (confirmar no SSMS).
 */
const areasLayout = [
  { name: "DML", wid: 53683 },
  { name: "HALL ELEVADORES / RECEPÇÃO", wid: 64025 },
  { name: "OPERAÇÕES", wid: null },
  { name: "RECEPÇÃO", wid: 64031 },
  { name: "SALA REUNIÃO 1", wid: 53687 },
  { name: "SALA REUNIÃO 2", wid: 53689 },
  { name: "SALA REUNIÃO 3", wid: 53695 },
  { name: "SALA TREINAMENTO/UNILIMP", wid: 53697 },
  { name: "SALÃO", wid: 64029 },
  { name: "WC COLETIVO FEM", wid: 63947 },
  { name: "WC DIRETORIA", wid: 63949 },
  { name: "WC FEM 1", wid: 63941 },
  { name: "WC MASC 1", wid: 63943 },
  { name: "WC MASC 2", wid: null },
  { name: "COMERCIAL / MARKETING", wid: 63951 },
  { name: "COMERCIAL / MARKETING", wid: 63953 },
  { name: "BANHEIROS LADO DIREITO", wid: 64033 },
  { name: "COPA", wid: 53685 },
  { name: "CORREDORES", wid: 64035 },
  { name: "DEPARTAMENTO FINANCEIRO", wid: 64027 },
  { name: "DIRETORIA", wid: 64195 },
];

const totalM2ByWid = { 53695: 40 };

const payload = areasLayout.map((area, idx) => {
  const wid = area.wid;
  const items =
    wid == null
      ? []
      : tasks
          .filter((t) => t.workAreaId === wid)
          .sort((a, b) => a.tarefa - b.tarefa)
          .map((t) => ({
            id: String(t.tarefa),
            name: t.descricao,
            orderBy: t.tarefa,
            frequency: {
              type: freqType(t.dias),
              weekDays: weekDays(t.dias),
            },
          }));

  return {
    id: "",
    name: area.name,
    description: "",
    quickTask: false,
    totalM2: wid != null ? totalM2ByWid[wid] ?? 0 : 0,
    employeeId: EMPLOYEE_PLACEHOLDER,
    headerId: wid != null ? String(wid) : "",
    orderBy: idx + 1,
    projectId: PROJECT_ID,
    items,
  };
});

fs.writeFileSync(outPath, JSON.stringify(payload, null, 2) + "\n", "utf8");
console.log("Escrito:", outPath, "| áreas:", payload.length, "| tarefas:", tasks.length);
