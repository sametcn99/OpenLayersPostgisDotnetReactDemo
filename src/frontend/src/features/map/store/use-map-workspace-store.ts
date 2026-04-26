import type { Geometry } from "geojson";
import { create } from "zustand";

import type {
  DrawingTool,
  EditorSelection,
  MapFeatureGeoJson,
} from "@/features/map/types";

interface MapWorkspaceState {
  tool: DrawingTool;
  features: MapFeatureGeoJson[];
  selection: EditorSelection;
  editorName: string;
  editorDescription: string;
  editorGeometry: Geometry | null;
  isLoading: boolean;
  isSaving: boolean;
  errorMessage: string | null;
  searchQuery: string;
  setTool: (tool: DrawingTool) => void;
  setFeatures: (features: MapFeatureGeoJson[]) => void;
  selectPersistedFeature: (feature: MapFeatureGeoJson) => void;
  startDraft: (geometry: Geometry) => void;
  updateEditorGeometry: (geometry: Geometry) => void;
  updateEditorName: (name: string) => void;
  updateEditorDescription: (description: string) => void;
  applySavedFeature: (feature: MapFeatureGeoJson) => void;
  removeFeature: (id: string) => void;
  clearEditor: () => void;
  setLoading: (value: boolean) => void;
  setSaving: (value: boolean) => void;
  setErrorMessage: (value: string | null) => void;
  setSearchQuery: (value: string) => void;
}

const emptyEditorState = {
  selection: null,
  editorName: "",
  editorDescription: "",
  editorGeometry: null,
} as const;

export const useMapWorkspaceStore = create<MapWorkspaceState>((set) => ({
  tool: "select",
  features: [],
  selection: null,
  editorName: "",
  editorDescription: "",
  editorGeometry: null,
  isLoading: true,
  isSaving: false,
  errorMessage: null,
  searchQuery: "",
  setTool: (tool) => set({ tool }),
  setFeatures: (features) => set({ features }),
  selectPersistedFeature: (feature) =>
    set({
      selection: { kind: "persisted", id: feature.id },
      editorName: feature.properties.name,
      editorDescription: feature.properties.description ?? "",
      editorGeometry: feature.geometry,
      errorMessage: null,
    }),
  startDraft: (geometry) =>
    set({
      selection: { kind: "draft" },
      editorName: "",
      editorDescription: "",
      editorGeometry: geometry,
      tool: "select",
      errorMessage: null,
    }),
  updateEditorGeometry: (geometry) => set({ editorGeometry: geometry }),
  updateEditorName: (editorName) => set({ editorName }),
  updateEditorDescription: (editorDescription) => set({ editorDescription }),
  applySavedFeature: (feature) =>
    set((state) => {
      const nextFeatures = state.features.filter(
        (entry) => entry.id !== feature.id,
      );

      nextFeatures.push(feature);
      nextFeatures.sort((left, right) =>
        left.properties.name.localeCompare(right.properties.name),
      );

      return {
        features: nextFeatures,
        selection: { kind: "persisted", id: feature.id },
        editorName: feature.properties.name,
        editorDescription: feature.properties.description ?? "",
        editorGeometry: feature.geometry,
        errorMessage: null,
      };
    }),
  removeFeature: (id) =>
    set((state) => {
      const nextState = {
        features: state.features.filter((feature) => feature.id !== id),
      };

      if (state.selection?.kind === "persisted" && state.selection.id === id) {
        return {
          ...nextState,
          ...emptyEditorState,
        };
      }

      return nextState;
    }),
  clearEditor: () => set({ ...emptyEditorState, errorMessage: null }),
  setLoading: (isLoading) => set({ isLoading }),
  setSaving: (isSaving) => set({ isSaving }),
  setErrorMessage: (errorMessage) => set({ errorMessage }),
  setSearchQuery: (searchQuery) => set({ searchQuery }),
}));
