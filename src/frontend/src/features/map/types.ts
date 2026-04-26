import type { Feature, FeatureCollection, Geometry } from "geojson";

export type DrawingTool = "select" | "Point" | "LineString" | "Polygon";

export interface MapFeatureProperties {
  id: string;
  name: string;
  description?: string | null;
  geometryType: string;
  source: "Seed" | "User";
  createdAtUtc: string;
  updatedAtUtc: string;
}

export type MapFeatureGeoJson = Feature<Geometry, MapFeatureProperties> & {
  id: string;
};

export type MapFeatureCollection = FeatureCollection<
  Geometry,
  MapFeatureProperties
> & {
  features: MapFeatureGeoJson[];
};

export type EditorSelection =
  | { kind: "persisted"; id: string }
  | { kind: "draft" }
  | null;

export interface SaveMapFeaturePayload {
  name: string;
  description?: string | null;
  geometry: Geometry;
}
