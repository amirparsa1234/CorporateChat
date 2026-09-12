(function () {
    const configuredApiBaseUrl =
        window.CORPORATE_CHAT_CONFIG?.apiBaseUrl || "/api";

    window.apiBaseUrl = configuredApiBaseUrl.replace(/\/+$/, "");

    window.CORPORATE_CHAT_CONFIG = {
        ...(window.CORPORATE_CHAT_CONFIG || {}),
        apiBaseUrl: window.apiBaseUrl
    };
})();
