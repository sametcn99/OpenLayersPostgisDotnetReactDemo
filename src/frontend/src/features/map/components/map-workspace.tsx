"use client";

import "ol/ol.css";

import { useDeferredValue, useMemo } from "react";

import { ControlPanel } from "@/features/map/components/control-panel";
import { MapSurface } from "@/features/map/components/map-surface";
import { useMapFeatureActions } from "@/features/map/hooks/use-map-feature-actions";
import { useOpenLayersMap } from "@/features/map/hooks/use-openlayers-map";
import { useMapWorkspaceStore } from "@/features/map/store/use-map-workspace-store";
import type { MapFeatureGeoJson } from "@/features/map/types";

export function MapWorkspace() {
  const {
    tool,
    features,
    selection,
    editorName,
    editorDescription,
    editorGeometry,
    isLoading,
    isSaving,
    errorMessage,
    searchQuery,
    setTool,
    setFeatures,
    selectPersistedFeature,
    startDraft,
    updateEditorGeometry,
    updateEditorName,
    updateEditorDescription,
    applySavedFeature,
    removeFeature,
    clearEditor,
    setLoading,
    setSaving,
    setErrorMessage,
    setSearchQuery,
  } = useMapWorkspaceStore();

  const { mapElementRef, clearMapSelection, removeDraftFeature } =
    useOpenLayersMap({
      tool,
      features,
      selection,
      setTool,
      clearEditor,
      selectPersistedFeature,
      startDraft,
      updateEditorGeometry,
    });

  const { loadFeatures, saveFeature, deleteFeature } = useMapFeatureActions({
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
  });

  const deferredSearchQuery = useDeferredValue(
    searchQuery.trim().toLowerCase(),
  );

  const visibleFeatures = useMemo(
    () =>
      features.filter((feature) => {
        if (!deferredSearchQuery) {
          return true;
        }

        const searchableText = `${feature.properties.name} ${feature.properties.description ?? ""}`;
        return searchableText.toLowerCase().includes(deferredSearchQuery);
      }),
    [deferredSearchQuery, features],
  );

  const discardDraft = () => {
    removeDraftFeature();
    clearMapSelection();
    clearEditor();
  };

  const selectFeature = (feature: MapFeatureGeoJson) => {
    removeDraftFeature();
    selectPersistedFeature(feature);
  };

  return (
    <main className="flex h-dvh w-full overflow-hidden p-4 lg:p-5">
      <div className="grid h-full w-full grid-cols-[minmax(360px,30%)_minmax(0,1fr)] gap-4 overflow-hidden rounded-4xl border border-(--panel-border) bg-(--panel) p-3 shadow-(--shadow) backdrop-blur-xl">
        <ControlPanel
          editorDescription={editorDescription}
          editorGeometry={editorGeometry}
          editorName={editorName}
          errorMessage={errorMessage}
          features={features}
          isLoading={isLoading}
          isSaving={isSaving}
          onDelete={() => void deleteFeature()}
          onDescriptionChange={updateEditorDescription}
          onDiscardDraft={discardDraft}
          onNameChange={updateEditorName}
          onRefresh={() => void loadFeatures()}
          onSave={() => void saveFeature()}
          onSearchChange={setSearchQuery}
          onSelectFeature={selectFeature}
          onToolChange={setTool}
          searchQuery={searchQuery}
          selection={selection}
          tool={tool}
          visibleFeatures={visibleFeatures}
        />

        <MapSurface mapElementRef={mapElementRef} />
      </div>
    </main>
  );
}
