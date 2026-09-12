const apiBaseUrl =
  window.apiBaseUrl ||
  window.CORPORATE_CHAT_CONFIG?.apiBaseUrl ||
  "/api";

window.apiBaseUrl = apiBaseUrl.replace(/\/+$/, "");

async function readResponse(response) {
  const text = await response.text();

  if (!text) {
    return null;
  }

  try {
    return JSON.parse(text);
  } catch {
    return text;
  }
}

function getErrorMessage(data, status) {
  if (typeof data === "string" && data.trim()) {
    return data;
  }

  const errors =
    data &&
    typeof data === "object" &&
    data.errors &&
    typeof data.errors === "object"
      ? Object.values(data.errors).flat().filter(Boolean).join(", ")
      : "";

  return (
    data?.message ||
    data?.title ||
    errors ||
    `Request failed: ${status}`
  );
}

async function apiRequest(path, options = {}) {
  const response = await fetch(`${window.apiBaseUrl}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {})
    }
  });

  const data = await readResponse(response);

  if (!response.ok) {
    throw new Error(getErrorMessage(data, response.status));
  }

  return data;
}

window.setToken = function (token) {
  localStorage.setItem("token", token);
};

window.getToken = function () {
  return localStorage.getItem("token");
};

window.setRefreshToken = function (refreshToken) {
  localStorage.setItem("refreshToken", refreshToken);
};

window.getRefreshToken = function () {
  return localStorage.getItem("refreshToken");
};

window.setAuthTokens = function (data) {
  if (data?.token) {
    window.setToken(data.token);
  }

  if (data?.refreshToken) {
    window.setRefreshToken(data.refreshToken);
  }
};

window.clearToken = function () {
  localStorage.removeItem("token");
  localStorage.removeItem("refreshToken");
};

window.authService = {
  register: async function (userName, password, email) {
    const data = await apiRequest("/Auth/Register", {
      method: "POST",
      body: JSON.stringify({
        userName: userName.trim(),
        password: password,
        email: email.trim()
      })
    });

    window.setAuthTokens(data);

    return data;
  },

  login: async function (userName, password) {
    const data = await apiRequest("/Auth/Login", {
      method: "POST",
      body: JSON.stringify({
        userName: userName.trim(),
        password: password
      })
    });

    window.setAuthTokens(data);

    return data;
  },

  refresh: async function (refreshToken) {
    const token = refreshToken || window.getRefreshToken();

    if (!token) {
      throw new Error("Refresh token not found");
    }

    const data = await apiRequest("/Auth/Refresh", {
      method: "POST",
      body: JSON.stringify({
        refreshToken: token
      })
    });

    window.setAuthTokens(data);

    return data;
  },

  logout: function () {
    window.clearToken();
    window.location.href = "login.html";
  }
};
