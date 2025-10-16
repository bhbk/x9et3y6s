import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  // Login page can be prerendered
  { path: 'login', renderMode: RenderMode.Prerender },

  // All other routes require authentication - client-side only
  { path: '**', renderMode: RenderMode.Client }
];
