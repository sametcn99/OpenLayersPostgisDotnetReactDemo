import type { Geometry as GeoJsonGeometry } from "geojson";
import type Feature from "ol/Feature";
import GeoJSON from "ol/format/GeoJSON";
import type Draw from "ol/interaction/Draw";
import { defaults as defaultInteractions } from "ol/interaction/defaults";
import Modify from "ol/interaction/Modify";
import Select from "ol/interaction/Select";
import TileLayer from "ol/layer/Tile";
import VectorLayer from "ol/layer/Vector";
import OlMap from "ol/Map";
import { fromLonLat } from "ol/proj";
import OSM from "ol/source/OSM";
import VectorSource from "ol/source/Vector";
import View from "ol/View";
import { useCallback, useEffect, useRef } from "react";

import { useOpenLayersDrawInteraction } from "@/features/map/hooks/use-openlayers-draw-interaction";
import { useOpenLayersSourceSync } from "@/features/map/hooks/use-openlayers-source-sync";
import { draftStyle, layerStyle } from "@/features/map/lib/openlayers-styles";
import type {
  DrawingTool,
  EditorSelection,
  MapFeatureGeoJson,
} from "@/features/map/types";

const featureFormat = new GeoJSON();

interface UseOpenLayersMapOptions {
  tool: DrawingTool;
  features: MapFeatureGeoJson[];
  selection: EditorSelection;
  setTool: (tool: DrawingTool) => void;
  clearEditor: () => void;
  selectPersistedFeature: (feature: MapFeatureGeoJson) => void;
  startDraft: (geometry: GeoJsonGeometry) => void;
  updateEditorGeometry: (geometry: GeoJsonGeometry) => void;
}

export function useOpenLayersMap({
  tool,
  features,
  selection,
  setTool,
  clearEditor,
  selectPersistedFeature,
  startDraft,
  updateEditorGeometry,
}: UseOpenLayersMapOptions) {
  const mapElementRef = useRef<HTMLDivElement | null>(null);
  const mapRef = useRef<OlMap | null>(null);
  const vectorSourceRef = useRef<VectorSource<Feature> | null>(null);
  const selectInteractionRef = useRef<Select | null>(null);
  const drawInteractionRef = useRef<Draw | null>(null);
  const draftFeatureRef = useRef<Feature | null>(null);
  const hasFittedInitialExtentRef = useRef(false);

  const removeDraftFeature = useCallback(() => {
    if (!draftFeatureRef.current || !vectorSourceRef.current) {
      return;
    }

    vectorSourceRef.current.removeFeature(draftFeatureRef.current);
    draftFeatureRef.current = null;
  }, []);

  const exitDrawMode = useCallback(() => {
    setTool("select");
  }, [setTool]);

  useEffect(() => {
    if (!mapElementRef.current || mapRef.current) {
      return;
    }

    const vectorSource = new VectorSource<Feature>();
    const vectorLayer = new VectorLayer({
      source: vectorSource,
      style: (feature) => (feature.get("isDraft") ? draftStyle : layerStyle),
    });
    const selectInteraction = new Select();
    const modifyInteraction = new Modify({
      features: selectInteraction.getFeatures(),
    });

    const map = new OlMap({
      target: mapElementRef.current,
      layers: [new TileLayer({ source: new OSM() }), vectorLayer],
      interactions: defaultInteractions().extend([
        selectInteraction,
        modifyInteraction,
      ]),
      view: new View({ center: fromLonLat([28.9784, 41.0082]), zoom: 11 }),
    });

    selectInteraction.on("select", (event) => {
      const selectedFeature = event.selected.at(0);

      if (!selectedFeature) {
        clearEditor();
        return;
      }

      if (selectedFeature.get("isDraft")) {
        const draftObject = featureFormat.writeFeatureObject(selectedFeature, {
          dataProjection: "EPSG:4326",
          featureProjection: "EPSG:3857",
        }) as { geometry: GeoJsonGeometry };

        startDraft(draftObject.geometry);
        return;
      }

      removeDraftFeature();
      const serverFeature = selectedFeature.get("serverFeature") as
        | MapFeatureGeoJson
        | undefined;
      if (serverFeature) {
        selectPersistedFeature(serverFeature);
      }
    });

    modifyInteraction.on("modifyend", (event) => {
      const modifiedFeature = event.features.item(0);
      if (!modifiedFeature) {
        return;
      }

      const featureObject = featureFormat.writeFeatureObject(modifiedFeature, {
        dataProjection: "EPSG:4326",
        featureProjection: "EPSG:3857",
      }) as { geometry: GeoJsonGeometry };

      updateEditorGeometry(featureObject.geometry);
    });

    vectorSourceRef.current = vectorSource;
    selectInteractionRef.current = selectInteraction;
    mapRef.current = map;

    return () => {
      map.setTarget(undefined);
      drawInteractionRef.current = null;
      selectInteractionRef.current = null;
      vectorSourceRef.current = null;
      mapRef.current = null;
    };
  }, [
    clearEditor,
    removeDraftFeature,
    selectPersistedFeature,
    startDraft,
    updateEditorGeometry,
  ]);

  useOpenLayersSourceSync({
    features,
    selection,
    mapRef,
    vectorSourceRef,
    selectInteractionRef,
    draftFeatureRef,
    hasFittedInitialExtentRef,
  });

  useOpenLayersDrawInteraction({
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
  });

  const clearMapSelection = useCallback(() => {
    selectInteractionRef.current?.getFeatures().clear();
  }, []);

  return { mapElementRef, clearMapSelection, removeDraftFeature };
}
