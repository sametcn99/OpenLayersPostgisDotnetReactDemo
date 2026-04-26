import type { Extent } from "ol/extent";
import type Feature from "ol/Feature";
import GeoJSON from "ol/format/GeoJSON";
import type Select from "ol/interaction/Select";
import type OlMap from "ol/Map";
import type VectorSource from "ol/source/Vector";
import { useCallback, useEffect } from "react";

import type { EditorSelection, MapFeatureGeoJson } from "@/features/map/types";

const featureFormat = new GeoJSON();

interface WritableRef<T> {
  current: T;
}

interface UseOpenLayersSourceSyncOptions {
  features: MapFeatureGeoJson[];
  selection: EditorSelection;
  mapRef: WritableRef<OlMap | null>;
  vectorSourceRef: WritableRef<VectorSource<Feature> | null>;
  selectInteractionRef: WritableRef<Select | null>;
  draftFeatureRef: WritableRef<Feature | null>;
  hasFittedInitialExtentRef: WritableRef<boolean>;
}

export function useOpenLayersSourceSync({
  features,
  selection,
  mapRef,
  vectorSourceRef,
  selectInteractionRef,
  draftFeatureRef,
  hasFittedInitialExtentRef,
}: UseOpenLayersSourceSyncOptions) {
  const syncSelectionOnMap = useCallback(
    (featureId: string | null) => {
      const source = vectorSourceRef.current;
      const selectInteraction = selectInteractionRef.current;

      if (!source || !selectInteraction) {
        return;
      }

      const selectedFeatures = selectInteraction.getFeatures();
      selectedFeatures.clear();

      if (!featureId) {
        return;
      }
      const targetFeature = source.getFeatureById(featureId);
      const extent = targetFeature?.getGeometry()?.getExtent();
      if (targetFeature && extent) {
        selectedFeatures.push(targetFeature);
        mapRef.current?.getView().fit(extent, {
          padding: [80, 80, 80, 80],
          duration: 400,
          maxZoom: 15,
        });
      }
    },
    [mapRef, selectInteractionRef, vectorSourceRef],
  );

  useEffect(() => {
    const source = vectorSourceRef.current;
    if (!source) {
      return;
    }

    source.clear();
    const olFeatures = featureFormat.readFeatures(
      { type: "FeatureCollection", features },
      { dataProjection: "EPSG:4326", featureProjection: "EPSG:3857" },
    );

    olFeatures.forEach((feature, index) => {
      const geoJsonFeature = features[index];
      feature.setId(geoJsonFeature.id);
      feature.set("serverFeature", geoJsonFeature);
      source.addFeature(feature);
    });

    if (draftFeatureRef.current) {
      source.addFeature(draftFeatureRef.current);
    }

    if (!hasFittedInitialExtentRef.current && olFeatures.length > 0) {
      hasFittedInitialExtentRef.current = true;
      const extent = source.getExtent();
      if (extent === null) {
        return;
      }

      mapRef.current?.getView().fit(extent satisfies Extent, {
        padding: [64, 64, 64, 64],
        duration: 450,
        maxZoom: 13,
      });
    }
  }, [
    draftFeatureRef,
    features,
    hasFittedInitialExtentRef,
    mapRef,
    vectorSourceRef,
  ]);

  useEffect(() => {
    syncSelectionOnMap(selection?.kind === "persisted" ? selection.id : null);
  }, [selection, syncSelectionOnMap]);
}
