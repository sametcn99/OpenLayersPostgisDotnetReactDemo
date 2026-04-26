import CircleStyle from "ol/style/Circle";
import Fill from "ol/style/Fill";
import Stroke from "ol/style/Stroke";
import Style from "ol/style/Style";

export const layerStyle = new Style({
  stroke: new Stroke({
    color: "#435160",
    width: 2,
  }),
  fill: new Fill({
    color: "rgba(67, 81, 96, 0.08)",
  }),
  image: new CircleStyle({
    radius: 5,
    fill: new Fill({ color: "rgba(255, 255, 255, 0.94)" }),
    stroke: new Stroke({ color: "#435160", width: 2 }),
  }),
});

export const draftStyle = new Style({
  stroke: new Stroke({
    color: "#6f7e8c",
    lineDash: [8, 8],
    width: 1.75,
  }),
  fill: new Fill({
    color: "rgba(111, 126, 140, 0.06)",
  }),
  image: new CircleStyle({
    radius: 5,
    fill: new Fill({ color: "rgba(247, 249, 250, 0.96)" }),
    stroke: new Stroke({ color: "#6f7e8c", width: 1.75 }),
  }),
});
