import { useState, useEffect } from "react";

interface SiteBranding {
  siteName: string;
  logoUrl: string;
  siteDescription: string;
}

const DEFAULT_BRANDING: SiteBranding = {
  siteName: "OpenDeepWiki",
  logoUrl: "/favicon.png",
  siteDescription: "AI 驱动的代码知识库",
};

let cachedBranding: SiteBranding | null = null;
let brandingPromise: Promise<SiteBranding> | null = null;

async function fetchBranding(): Promise<SiteBranding> {
  if (cachedBranding) return cachedBranding;
  if (!brandingPromise) {
    brandingPromise = (async () => {
      try {
        const res = await fetch("/api/system/branding", { cache: "force-cache" });
        if (res.ok) {
          const data = await res.json();
          cachedBranding = {
            siteName: data.siteName || DEFAULT_BRANDING.siteName,
            logoUrl: data.logoUrl || DEFAULT_BRANDING.logoUrl,
            siteDescription: data.siteDescription || DEFAULT_BRANDING.siteDescription,
          };
        }
      } catch {
        // Fallback to defaults
      }
      brandingPromise = null;
      return cachedBranding ?? DEFAULT_BRANDING;
    })();
  }
  return brandingPromise;
}

export function useSiteBranding(): SiteBranding {
  const [branding, setBranding] = useState<SiteBranding>(DEFAULT_BRANDING);

  useEffect(() => {
    fetchBranding().then(setBranding);
  }, []);

  return branding;
}
