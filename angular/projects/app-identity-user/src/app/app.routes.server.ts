import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  // Static public pages - prerender at build time
  { path: 'login', renderMode: RenderMode.Prerender },
  { path: 'forgot-password', renderMode: RenderMode.Prerender },

  // All other routes - client-side only
  // (includes routes with params and authenticated pages)
  { path: '**', renderMode: RenderMode.Client }
];
