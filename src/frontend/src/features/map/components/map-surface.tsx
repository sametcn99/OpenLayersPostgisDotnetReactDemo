import type { RefObject } from "react";

interface MapSurfaceProps {
  mapElementRef: RefObject<HTMLDivElement | null>;
}

export function MapSurface({ mapElementRef }: MapSurfaceProps) {
  return (
    <section className="relative min-h-0 overflow-hidden rounded-[1.6rem] border border-[rgba(24,34,47,0.08)] bg-[#e6ebe7]">
      <div className="map-surface h-full w-full" ref={mapElementRef} />
    </section>
  );
}
