import type { Metadata } from "next";
import RouteProviders from "@/app/route-providers";
import "./globals.css";

export const metadata: Metadata = {
  title: "OpenDeepWiki",
  description: "AI 驱动的代码知识库，用于仓库分析和文档生成",
  icons: {
    icon: "/favicon.png",
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="zh" suppressHydrationWarning>
      <body className="antialiased">
        <RouteProviders>{children}</RouteProviders>
      </body>
    </html>
  );
}
