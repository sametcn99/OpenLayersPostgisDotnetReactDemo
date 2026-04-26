import type {
  MapFeatureCollection,
  MapFeatureGeoJson,
  SaveMapFeaturePayload,
} from "@/features/map/types";

const apiBaseUrl =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ??
  "http://localhost:8080";

function buildUrl(path: string): string {
  return `${apiBaseUrl}${path}`;
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.text();
    throw new Error(body || `Request failed with status ${response.status}.`);
  }

  return (await response.json()) as T;
}

export async function listMapFeatures(): Promise<MapFeatureCollection> {
  const response = await fetch(buildUrl("/api/map-features"), {
    cache: "no-store",
  });

  return handleResponse<MapFeatureCollection>(response);
}

export async function createMapFeature(
  payload: SaveMapFeaturePayload,
): Promise<MapFeatureGeoJson> {
  const response = await fetch(buildUrl("/api/map-features"), {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  return handleResponse<MapFeatureGeoJson>(response);
}

export async function updateMapFeature(
  id: string,
  payload: SaveMapFeaturePayload,
): Promise<MapFeatureGeoJson> {
  const response = await fetch(buildUrl(`/api/map-features/${id}`), {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  return handleResponse<MapFeatureGeoJson>(response);
}

export async function deleteMapFeature(id: string): Promise<void> {
  const response = await fetch(buildUrl(`/api/map-features/${id}`), {
    method: "DELETE",
  });

  if (!response.ok && response.status !== 204) {
    const body = await response.text();
    throw new Error(body || `Delete failed with status ${response.status}.`);
  }
}
