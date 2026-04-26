import type { Geometry as GeoJsonGeometry } from "geojson";

import type { EditorSelection } from "@/features/map/types";

interface EditorPanelProps {
  selection: EditorSelection;
  editorName: string;
  editorDescription: string;
  editorGeometry: GeoJsonGeometry | null;
  isSaving: boolean;
  errorMessage: string | null;
  onNameChange: (value: string) => void;
  onDescriptionChange: (value: string) => void;
  onSave: () => void;
  onDelete: () => void;
  onDiscardDraft: () => void;
}

export function EditorPanel({
  selection,
  editorName,
  editorDescription,
  editorGeometry,
  isSaving,
  errorMessage,
  onNameChange,
  onDescriptionChange,
  onSave,
  onDelete,
  onDiscardDraft,
}: EditorPanelProps) {
  return (
    <div className="rounded-[1.4rem] border border-(--panel-border) bg-white/70 p-4">
      <div className="flex items-center justify-between gap-3">
        <div>
          <h2 className="text-sm font-semibold uppercase tracking-[0.22em] text-(--muted)">
            Editor
          </h2>
          <p className="mt-1 text-sm text-(--muted)">
            {selection?.kind === "persisted"
              ? "Selected feature is ready for update or removal."
              : selection?.kind === "draft"
                ? "Draft geometry is waiting to be saved."
                : "Draw or select a feature to start editing."}
          </p>
        </div>

        {selection?.kind === "draft" ? (
          <button
            className="rounded-full border border-(--panel-border) px-3 py-1.5 text-xs font-semibold text-foreground transition hover:bg-(--accent-soft)"
            onClick={onDiscardDraft}
            type="button"
          >
            Discard Draft
          </button>
        ) : null}
      </div>

      <div className="mt-4 space-y-3">
        <label className="block text-sm font-medium text-foreground">
          Name
          <input
            className="mt-2 w-full rounded-2xl border border-(--panel-border) bg-white px-4 py-3 outline-none transition focus:border-(--accent)"
            onChange={(event) => onNameChange(event.target.value)}
            placeholder="Historic square"
            value={editorName}
          />
        </label>

        <label className="block text-sm font-medium text-foreground">
          Description
          <textarea
            className="mt-2 min-h-28 w-full resize-none rounded-2xl border border-(--panel-border) bg-white px-4 py-3 outline-none transition focus:border-(--accent)"
            onChange={(event) => onDescriptionChange(event.target.value)}
            placeholder="What does this feature represent on the map?"
            value={editorDescription}
          />
        </label>

        <div className="grid grid-cols-2 gap-3 text-xs text-(--muted)">
          <div className="rounded-2xl bg-(--accent-soft) px-3 py-2">
            <p className="font-semibold uppercase tracking-[0.18em] text-(--accent-strong)">
              Geometry
            </p>
            <p className="mt-1 text-sm text-foreground">
              {editorGeometry?.type ?? "None"}
            </p>
          </div>
          <div className="rounded-2xl bg-black/4 px-3 py-2">
            <p className="font-semibold uppercase tracking-[0.18em] text-(--muted)">
              Status
            </p>
            <p className="mt-1 text-sm text-foreground">
              {selection?.kind === "persisted"
                ? "Saved feature"
                : selection?.kind === "draft"
                  ? "Unsaved draft"
                  : "Waiting"}
            </p>
          </div>
        </div>

        <div className="flex gap-3">
          <button
            className="flex-1 rounded-full bg-(--accent) px-4 py-3 text-sm font-semibold text-white transition hover:bg-(--accent-strong) disabled:cursor-not-allowed disabled:opacity-60"
            disabled={!editorGeometry || isSaving}
            onClick={onSave}
            type="button"
          >
            {isSaving
              ? "Saving..."
              : selection?.kind === "persisted"
                ? "Update Feature"
                : "Save Feature"}
          </button>

          <button
            className="rounded-full border border-(--panel-border) px-4 py-3 text-sm font-semibold text-foreground transition hover:bg-black/5 disabled:cursor-not-allowed disabled:opacity-50"
            disabled={selection?.kind !== "persisted" || isSaving}
            onClick={onDelete}
            type="button"
          >
            Delete
          </button>
        </div>

        {errorMessage ? (
          <div className="rounded-2xl border border-[#c4644c]/20 bg-[#fff0eb] px-4 py-3 text-sm text-[#8e3e2a]">
            {errorMessage}
          </div>
        ) : null}
      </div>
    </div>
  );
}
