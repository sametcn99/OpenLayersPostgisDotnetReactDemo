import type { Geometry as GeoJsonGeometry } from "geojson";

import { DrawingTools } from "@/features/map/components/drawing-tools";
import { EditorPanel } from "@/features/map/components/editor-panel";
import { FeatureList } from "@/features/map/components/feature-list";
import type {
  DrawingTool,
  EditorSelection,
  MapFeatureGeoJson,
} from "@/features/map/types";

interface ControlPanelProps {
  tool: DrawingTool;
  features: MapFeatureGeoJson[];
  visibleFeatures: MapFeatureGeoJson[];
  selection: EditorSelection;
  editorName: string;
  editorDescription: string;
  editorGeometry: GeoJsonGeometry | null;
  isLoading: boolean;
  isSaving: boolean;
  errorMessage: string | null;
  searchQuery: string;
  onToolChange: (tool: DrawingTool) => void;
  onRefresh: () => void;
  onNameChange: (value: string) => void;
  onDescriptionChange: (value: string) => void;
  onSave: () => void;
  onDelete: () => void;
  onDiscardDraft: () => void;
  onSearchChange: (value: string) => void;
  onSelectFeature: (feature: MapFeatureGeoJson) => void;
}

export function ControlPanel(props: ControlPanelProps) {
  return (
    <aside className="flex min-h-0 flex-col overflow-y-auto overscroll-contain rounded-[1.6rem] border border-(--panel-border) bg-(--panel-strong) px-5 py-5">
      <div className="flex items-start justify-between gap-4 border-b border-(--panel-border) pb-4">
        <div className="space-y-2">
          <p className="text-[0.7rem] font-semibold uppercase tracking-[0.28em] text-(--accent)">
            Geo Demo Control Desk
          </p>
          <h1 className="font-(--font-display) text-3xl leading-tight text-foreground">
            Draw, inspect, and store GeoJSON features.
          </h1>
          <p className="max-w-sm text-sm leading-6 text-(--muted)">
            Seeded features load from PostGIS. New drawings stay on the map as
            drafts until you save them.
          </p>
        </div>

        <button
          className="rounded-full border border-(--panel-border) px-4 py-2 text-sm font-medium text-foreground transition hover:bg-(--accent-soft)"
          onClick={props.onRefresh}
          type="button"
        >
          Refresh
        </button>
      </div>

      <DrawingTools tool={props.tool} onToolChange={props.onToolChange} />

      <section className="mt-5 flex flex-col gap-4">
        <EditorPanel
          editorDescription={props.editorDescription}
          editorGeometry={props.editorGeometry}
          editorName={props.editorName}
          errorMessage={props.errorMessage}
          isSaving={props.isSaving}
          onDelete={props.onDelete}
          onDescriptionChange={props.onDescriptionChange}
          onDiscardDraft={props.onDiscardDraft}
          onNameChange={props.onNameChange}
          onSave={props.onSave}
          selection={props.selection}
        />

        <FeatureList
          features={props.features}
          isLoading={props.isLoading}
          onSearchChange={props.onSearchChange}
          onSelectFeature={props.onSelectFeature}
          searchQuery={props.searchQuery}
          selection={props.selection}
          visibleFeatures={props.visibleFeatures}
        />
      </section>
    </aside>
  );
}
