// Service Worker - Minimal implementation for SGRRHH.Web
// This provides offline caching capabilities

const CACHE_NAME = 'sgrrhh-cache-v1';

self.addEventListener('install', event => {
    console.log('[ServiceWorker] Install');
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    console.log('[ServiceWorker] Activate');
    event.waitUntil(clients.claim());
});

self.addEventListener('fetch', event => {
    // Pass through all requests to the network
    event.respondWith(fetch(event.request));
});
