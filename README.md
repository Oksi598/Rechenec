# Rechenec

## Локальна розробка (фронт + API)

З каталогу `front-Logistics`:

```bash
npm install
npm run dev:all
```

Відкрийте в браузері **http://127.0.0.1:5173** (той самий хост, що й проксі до API на `http://127.0.0.1:5212`).

Окремо: термінал 1 — `npm run dev:api`, термінал 2 — `npm run dev`.

Якщо бекенд уже запущено з HTTPS-профілем, у `front-Logistics/.env.development` вкажіть `VITE_API_PROXY=https://127.0.0.1:7246`.
