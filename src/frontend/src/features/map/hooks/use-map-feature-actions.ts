import type { Geometry as GeoJsonGeometry } from "geojson";
import { startTransition, useCallback, useEffect } from "react";

import {
  createMapFeature,
  deleteMapFeature,
  listMapFeatures,
  updateMapFeature,
} from "@/features/map/api/map-features-api";
import type {
  EditorSelection,
  MapFeatureGeoJson,
  SaveMapFeaturePayload,
} from "@/features/map/types";

function readErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return "An unexpected error occurred.";
}

function toPayload(
  name: string,
  description: string,
  geometry: GeoJsonGeometry,
): SaveMapFeaturePayload {
  return {
    name,
    description: description.trim() ? description.trim() : null,
    geometry,
  };
}

interface UseMapFeatureActionsOptions {
  selection: EditorSelection;
  editorName: string;
  editorDescription: string;
  editorGeometry: GeoJsonGeometry | null;
  setFeatures: (features: MapFeatureGeoJson[]) => void;
  applySavedFeature: (feature: MapFeatureGeoJson) => void;
  removeFeature: (id: string) => void;
  setLoading: (value: boolean) => void;
  setSaving: (value: boolean) => void;
  setErrorMessage: (value: string | null) => void;
  removeDraftFeature: () => void;
  clearMapSelection: () => void;
}

export function useMapFeatureActions({
  selection,
  editorName,
  editorDescription,
  editorGeometry,
  setFeatures,
  applySavedFeature,
  removeFeature,
  setLoading,
  setSaving,
  setErrorMessage,
  removeDraftFeature,
  clearMapSelection,
}: UseMapFeatureActionsOptions) {
  const loadFeatures = useCallback(async () => {
    try {
      setLoading(true);
      setErrorMessage(null);
      const response = await listMapFeatures();

      startTransition(() => {
        setFeatures(response.features);
      });
    } catch (error) {
      setErrorMessage(readErrorMessage(error));
    } finally {
      setLoading(false);
    }
  }, [setErrorMessage, setFeatures, setLoading]);

  useEffect(() => {
    void loadFeatures();
  }, [loadFeatures]);

  const saveFeature = async () => {
    if (!editorGeometry || editorName.trim().length < 2) {
      setErrorMessage(
        "Provide a feature name and a valid geometry before saving.",
      );
      return;
    }

    try {
      setSaving(true);
      setErrorMessage(null);

      const payload = toPayload(editorName, editorDescription, editorGeometry);
      const feature =
        selection?.kind === "persisted"
          ? await updateMapFeature(selection.id, payload)
          : await createMapFeature(payload);

      removeDraftFeature();
      startTransition(() => {
        applySavedFeature(feature);
      });
    } catch (error) {
      setErrorMessage(readErrorMessage(error));
    } finally {
      setSaving(false);
    }
  };

  const deleteFeature = async () => {
    if (selection?.kind !== "persisted") {
      return;
    }

    try {
      setSaving(true);
      setErrorMessage(null);
      await deleteMapFeature(selection.id);

      startTransition(() => {
        removeFeature(selection.id);
      });
      clearMapSelection();
    } catch (error) {
      setErrorMessage(readErrorMessage(error));
    } finally {
      setSaving(false);
    }
  };

  return { loadFeatures, saveFeature, deleteFeature };
}
