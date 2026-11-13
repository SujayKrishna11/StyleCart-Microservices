import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
    plugins: [react()],
    server: {
        watch: {
            ignored: ["**/.vs/**"],
        },
        proxy: {
            "/gateway": {
                target: "http://localhost:5270",
                changeOrigin: true,
            },
            "/api": {
                target: "http://localhost:5270",
                changeOrigin: true,
            },
        },
    },
});
