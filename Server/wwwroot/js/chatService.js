window.chatService = {
  getWithUser: async otherUserId => {
    const token = window.getToken();
    const payload = JSON.parse(atob(token.split('.')[1]));
    const userId = payload.sub;
    const res = await fetch(
      `${window.apiBaseUrl}/Messages/conversation?userId=${userId}&otherUserId=${otherUserId}`,
      { headers: { Authorization: `Bearer ${token}` } }
    );
    if (!res.ok) {
      const err = await res.json().catch(() => null);
      throw new Error(err?.message || `Load conversation failed: ${res.status}`);
    }
    return res.json();
  },
  getGroupMessages: async groupId => {
    const token = window.getToken();
    const res = await fetch(
      `${window.apiBaseUrl}/Groups/${groupId}/Messages`,
      { headers: { Authorization: `Bearer ${token}` } }
    );
    if (!res.ok) {
      const err = await res.json().catch(() => null);
      throw new Error(err?.message || `Load group messages failed: ${res.status}`);
    }
    return res.json();
  }
};
