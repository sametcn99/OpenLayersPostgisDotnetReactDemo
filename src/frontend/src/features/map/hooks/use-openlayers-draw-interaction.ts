import type { Geometry as GeoJsonGeometry } from "geojson";
import type Feature from "ol/Feature";
import GeoJSON from "ol/format/GeoJSON";
import Draw from "ol/interaction/Draw";
import type Select from "ol/interaction/Select";
import type OlMap from "ol/Map";
import type VectorSource from "ol/source/Vector";
import { useEffect } from "react";

import type { DrawingTool } from "@/features/map/types";

const featureFormat = new GeoJSON();

interface WritableRef<T> {
  current: T;
}

interface UseOpenLayersDrawInteractionOptions {
  tool: DrawingTool;
  mapRef: WritableRef<OlMap | null>;
  vectorSourceRef: WritableRef<VectorSource<Feature> | null>;
  selectInteractionRef: WritableRef<Select | null>;
  drawInteractionRef: WritableRef<Draw | null>;
  draftFeatureRef: WritableRef<Feature | null>;
  clearEditor: () => void;
  removeDraftFeature: () => void;
  startDraft: (geometry: GeoJsonGeometry) => void;
  exitDrawMode: () => void;
}

export function useOpenLayersDrawInteraction({
  tool,
  mapRef,
  vectorSourceRef,
  selectInteractionRef,
  drawInteractionRef,
  draftFeatureRef,
  clearEditor,
  removeDraftFeature,
  startDraft,
  exitDrawMode,
}: UseOpenLayersDrawInteractionOptions) {
  useEffect(() => {
    const map = mapRef.current;
    const source = vectorSourceRef.current;
    if (!map || !source) {
      return;
    }

    if (drawInteractionRef.current) {
      map.removeInteraction(drawInteractionRef.current);
      drawInteractionRef.current = null;
    }

    if (tool === "select") {
      return;
    }

    const drawInteraction = new Draw({ source, type: tool, stopClick: true });
    const exitActiveDrawing = () => {
      drawInteraction.abortDrawing();
      exitDrawMode();
    };
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key !== "Escape") {
        return;
      }

      event.preventDefault();
      exitActiveDrawing();
    };
    const handleContextMenu = (event: MouseEvent) => {
      event.preventDefault();
      exitActiveDrawing();
    };

    drawInteraction.on("drawstart", () => {
      removeDraftFeature();
      selectInteractionRef.current?.getFeatures().clear();
      clearEditor();
    });

    drawInteraction.on("drawend", (event) => {
      const drawnFeature = event.feature;
      drawnFeature.set("isDraft", true);
      draftFeatureRef.current = drawnFeature;

      const featureObject = featureFormat.writeFeatureObject(drawnFeature, {
        dataProjection: "EPSG:4326",
        featureProjection: "EPSG:3857",
      }) as { geometry: GeoJsonGeometry };

      startDraft(featureObject.geometry);
      selectInteractionRef.current?.getFeatures().clear();
      selectInteractionRef.current?.getFeatures().push(drawnFeature);
    });

    map.addInteraction(drawInteraction);
    window.addEventListener("keydown", handleKeyDown);
    map.getViewport().addEventListener("contextmenu", handleContextMenu);
    drawInteractionRef.current = drawInteraction;

    return () => {
      window.removeEventListener("keydown", handleKeyDown);
      map.getViewport().removeEventListener("contextmenu", handleContextMenu);
      map.removeInteraction(drawInteraction);
      drawInteractionRef.current = null;
    };
  }, [
    clearEditor,
    draftFeatureRef,
    drawInteractionRef,
    exitDrawMode,
    mapRef,
    removeDraftFeature,
    selectInteractionRef,
    startDraft,
    tool,
    vectorSourceRef,
  ]);
}
