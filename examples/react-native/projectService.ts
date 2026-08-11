import { BASE_URL } from './apiConfig';

/**
 * Contrato `frequency.weekDays` na API: **0 = domingo … 6 = sábado** (igual `Date.getDay()` no RN).
 * Se na UI domingo é o 1.º checkbox com índice 0, envie **0** para domingo — **não** use `índice + 1`:
 * isso grava [1,2,3,4,5] (seg–sex) em vez de [0,1,2,3,4] (dom–qui).
 */
export type ApiWeekDay = 0 | 1 | 2 | 3 | 4 | 5 | 6;

type ProjectByLegacyResponse = unknown;

function logRequest(method: string, url: string) {
  if (__DEV__) {
    console.log(`[API] ${method} ${url}`);
  }
}

function logError(context: string, error: unknown) {
  if (__DEV__) {
    console.error(`[API][${context}]`, error);
    if (error instanceof Error) {
      console.error('[API] message:', error.message, 'stack:', error.stack);
    }
  }
}

/**
 * Exemplo alinhado ao backend:
 * GET /v1/Project/legacyId/{legacyId}
 */
export async function getProjectByLegacyId(
  legacyId: number,
): Promise<ProjectByLegacyResponse> {
  const url = `${BASE_URL}/v1/Project/legacyId/${legacyId}`;
  logRequest('GET', url);

  try {
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        Accept: 'application/json',
      },
    });

    if (!response.ok) {
      const bodyText = await response.text().catch(() => '');
      const err = new Error(
        `HTTP ${response.status} ${response.statusText}${bodyText ? ` — ${bodyText}` : ''}`,
      );
      logError('getProjectByLegacyId response', err);
      throw err;
    }

    return (await response.json()) as ProjectByLegacyResponse;
  } catch (error) {
    logError('getProjectByLegacyId network', error);
    throw error;
  }
}
