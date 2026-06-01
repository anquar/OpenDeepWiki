"use client";

import { useState, useCallback } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { useTranslations } from "@/hooks/use-translations";
import {
  Copy,
  Check,
  Server,
} from "lucide-react";

interface IntegrationsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function IntegrationsDialog({ open, onOpenChange }: IntegrationsDialogProps) {
  const t = useTranslations();

  const [mcpUrlCopied, setMcpUrlCopied] = useState(false);
  const [configCopied, setConfigCopied] = useState(false);

  const mcpUrl = typeof window !== "undefined" ? `${window.location.origin}/api/mcp` : "/api/mcp";

  const claudeDesktopConfig = JSON.stringify(
    {
      mcpServers: {
        deepwiki: {
          url: mcpUrl,
        },
      },
    },
    null,
    2
  );

  const copyToClipboard = useCallback(async (text: string, type: "url" | "config") => {
    try {
      await navigator.clipboard.writeText(text);
      if (type === "url") {
        setMcpUrlCopied(true);
        setTimeout(() => setMcpUrlCopied(false), 2000);
      } else {
        setConfigCopied(true);
        setTimeout(() => setConfigCopied(false), 2000);
      }
    } catch {
      // Clipboard API not available
    }
  }, []);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{t("home.integrations.title")}</DialogTitle>
          <DialogDescription>{t("home.integrations.description")}</DialogDescription>
        </DialogHeader>

        <div className="space-y-4 pt-2">
          {/* MCP Server Section */}
          <div className="rounded-lg border p-4 space-y-3">
            <div className="flex items-center gap-2">
              <Server className="h-5 w-5 text-muted-foreground" />
              <h3 className="font-medium">{t("home.integrations.mcp.title")}</h3>
            </div>
            <p className="text-sm text-muted-foreground">{t("home.integrations.mcp.description")}</p>

            {/* MCP URL */}
            <div className="space-y-1.5">
              <label className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
                {t("home.integrations.mcp.serverUrl")}
              </label>
              <div className="flex items-center gap-2">
                <code className="flex-1 rounded-md bg-muted px-3 py-2 text-sm font-mono break-all">
                  {mcpUrl}
                </code>
                <Button
                  variant="outline"
                  size="sm"
                  className="shrink-0 gap-1.5"
                  onClick={() => copyToClipboard(mcpUrl, "url")}
                >
                  {mcpUrlCopied ? (
                    <>
                      <Check className="h-3.5 w-3.5" />
                      {t("home.integrations.mcp.copied")}
                    </>
                  ) : (
                    <>
                      <Copy className="h-3.5 w-3.5" />
                      {t("home.integrations.mcp.copyUrl")}
                    </>
                  )}
                </Button>
              </div>
            </div>

            {/* Claude Desktop Config */}
            <div className="space-y-1.5">
              <p className="text-sm text-muted-foreground">
                {t("home.integrations.mcp.claudeDesktopHint")}
              </p>
              <div className="relative">
                <pre className="rounded-md bg-muted px-3 py-2 text-xs font-mono overflow-x-auto">
                  {claudeDesktopConfig}
                </pre>
                <Button
                  variant="outline"
                  size="sm"
                  className="absolute top-1.5 right-1.5 h-7 gap-1 text-xs"
                  onClick={() => copyToClipboard(claudeDesktopConfig, "config")}
                >
                  {configCopied ? (
                    <>
                      <Check className="h-3 w-3" />
                      {t("home.integrations.mcp.copied")}
                    </>
                  ) : (
                    <>
                      <Copy className="h-3 w-3" />
                      {t("home.integrations.mcp.copyConfig")}
                    </>
                  )}
                </Button>
              </div>
            </div>

            {/* Claude.ai Hint */}
            <p className="text-sm text-muted-foreground">
              {t("home.integrations.mcp.claudeAiHint")}
            </p>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
