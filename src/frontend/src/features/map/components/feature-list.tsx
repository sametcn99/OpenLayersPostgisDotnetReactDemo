import type { EditorSelection, MapFeatureGeoJson } from "@/features/map/types";

interface FeatureListProps {
  features: MapFeatureGeoJson[];
  visibleFeatures: MapFeatureGeoJson[];
  selection: EditorSelection;
  isLoading: boolean;
  searchQuery: string;
  onSearchChange: (value: string) => void;
  onSelectFeature: (feature: MapFeatureGeoJson) => void;
}

export function FeatureList({
  features,
  visibleFeatures,
  selection,
  isLoading,
  searchQuery,
  onSearchChange,
  onSelectFeature,
}: FeatureListProps) {
  return (
    <div className="flex flex-col rounded-[1.4rem] border border-(--panel-border) bg-white/70 p-4">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h2 className="text-sm font-semibold uppercase tracking-[0.22em] text-(--muted)">
            Persisted Features
          </h2>
          <p className="mt-1 text-sm text-(--muted)">
            Seed and user features currently available in PostGIS.
          </p>
        </div>

        <span className="rounded-full bg-black/4 px-3 py-1 text-xs font-semibold text-foreground">
          {features.length} total
        </span>
      </div>

      <input
        className="mt-4 rounded-full border border-(--panel-border) bg-white px-4 py-3 text-sm outline-none transition focus:border-(--accent)"
        onChange={(event) => onSearchChange(event.target.value)}
        placeholder="Search features"
        value={searchQuery}
      />

      <div className="mt-4 flex flex-col gap-2">
        {isLoading ? (
          <div className="rounded-2xl border border-dashed border-(--panel-border) px-4 py-6 text-sm text-(--muted)">
            Loading saved features from the backend...
          </div>
        ) : null}

        {!isLoading && visibleFeatures.length === 0 ? (
          <div className="rounded-2xl border border-dashed border-(--panel-border) px-4 py-6 text-sm text-(--muted)">
            No persisted features match the current search.
          </div>
        ) : null}

        {visibleFeatures.map((feature) => {
          const isSelected =
            selection?.kind === "persisted" && selection.id === feature.id;

          return (
            <button
              className={`rounded-[1.2rem] border px-4 py-3 text-left transition ${
                isSelected
                  ? "border-transparent bg-(--accent) text-white shadow-lg"
                  : "border-(--panel-border) bg-white text-foreground hover:border-(--accent) hover:bg-(--accent-soft)"
              }`}
              key={feature.id}
              onClick={() => onSelectFeature(feature)}
              type="button"
            >
              <div className="flex items-center justify-between gap-3">
                <div>
                  <p className="font-semibold">{feature.properties.name}</p>
                  <p className="mt-1 text-xs opacity-75">
                    {feature.properties.description || "No description"}
                  </p>
                </div>

                <div className="text-right text-xs font-semibold uppercase tracking-[0.18em] opacity-75">
                  <p>{feature.properties.geometryType}</p>
                  <p className="mt-1">{feature.properties.source}</p>
                </div>
              </div>
            </button>
          );
        })}
      </div>
    </div>
  );
}
