import type { DrawingTool } from "@/features/map/types";

const drawingTools = [
  { label: "Select", value: "select" },
  { label: "Point", value: "Point" },
  { label: "Line", value: "LineString" },
  { label: "Polygon", value: "Polygon" },
] satisfies { label: string; value: DrawingTool }[];

interface DrawingToolsProps {
  tool: DrawingTool;
  onToolChange: (tool: DrawingTool) => void;
}

export function DrawingTools({ tool, onToolChange }: DrawingToolsProps) {
  return (
    <section className="mt-5 space-y-3">
      <div className="flex items-center justify-between">
        <h2 className="text-sm font-semibold uppercase tracking-[0.22em] text-(--muted)">
          Drawing Tools
        </h2>
        <span className="rounded-full bg-(--accent-soft) px-3 py-1 text-xs font-semibold text-(--accent-strong)">
          {tool === "select" ? "Selection Mode" : `${tool} Tool`}
        </span>
      </div>

      <div className="grid grid-cols-2 gap-2">
        {drawingTools.map((entry) => {
          const isActive = tool === entry.value;

          return (
            <button
              className={`rounded-2xl border px-4 py-3 text-left text-sm transition ${
                isActive
                  ? "border-transparent bg-(--accent) text-white shadow-lg"
                  : "border-(--panel-border) bg-white/70 text-foreground hover:border-(--accent) hover:bg-(--accent-soft)"
              }`}
              key={entry.value}
              onClick={() => onToolChange(entry.value)}
              type="button"
            >
              <span className="block font-semibold">{entry.label}</span>
              <span className="mt-1 block text-xs opacity-75">
                {entry.value === "select"
                  ? "Inspect or modify a selected geometry."
                  : `Draw a ${entry.label.toLowerCase()} on the map.`}
              </span>
            </button>
          );
        })}
      </div>
    </section>
  );
}
