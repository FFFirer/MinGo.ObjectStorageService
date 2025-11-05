import { defineConfig } from "vite"
import tailwindcss from '@tailwindcss/vite';
import solid from "vite-plugin-solid";

export default defineConfig({
    appType: 'custom',
    base: "/dist/",
    publicDir: false,
    build: {
        manifest: true,
        emptyOutDir: true,
        outDir: "wwwroot/dist",
        rollupOptions: {
            input: ["tailwind.config.css"]
        }
    },
    plugins: [
        tailwindcss(),
        solid()
    ]
})