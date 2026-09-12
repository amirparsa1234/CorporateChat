window.groupService = {
  getMy: async () => {
    const res = await fetch(`${window.apiBaseUrl}/Groups`, {
      headers: {
        Accept: 'application/json',
        Authorization: `Bearer ${window.getToken()}`
      }
    });
    if (!res.ok) {
      const err = await res.json().catch(() => null);
      throw new Error(err?.message || `Load groups failed: ${res.status}`);
    }
    return res.json();
  },
  create: async name => {
    const res = await fetch(`${window.apiBaseUrl}/Groups`, {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
        Authorization: `Bearer ${window.getToken()}`
      },
      body: JSON.stringify({ name })
    });
    const data = await res.json().catch(() => null);
    if (!res.ok) throw new Error(data?.message || `Create failed: ${res.status}`);
    return data;
  },
  getById: async id => {
    const res = await fetch(`${window.apiBaseUrl}/Groups/${id}`, {
      headers: {
        Accept: 'application/json',
        Authorization: `Bearer ${window.getToken()}`
      }
    });
    if (!res.ok) {
      const err = await res.json().catch(() => null);
      throw new Error(err?.message || `Load group failed: ${res.status}`);
    }
    return res.json();
  },
  addMember: async (groupId, userId, role) => {
    const res = await fetch(`${window.apiBaseUrl}/Groups/${groupId}/members`, {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
        Authorization: `Bearer ${window.getToken()}`
      },
      body: JSON.stringify({ userId, role })
    });
    if (!res.ok) {
      const err = await res.json().catch(() => null);
      throw new Error(err?.message || `Add member failed: ${res.status}`);
    }
  },
  removeMember: async (groupId, userId) => {
    const res = await fetch(`${window.apiBaseUrl}/Groups/${groupId}/members/${userId}`, {
      method: 'DELETE',
      headers: {
        Accept: 'application/json',
        Authorization: `Bearer ${window.getToken()}`
      }
    });
    if (!res.ok) {
      const err = await res.json().catch(() => null);
      throw new Error(err?.message || `Remove member failed: ${res.status}`);
    }
  },
  changeRole: async (groupId, userId, role) => {
    const res = await fetch(`${window.apiBaseUrl}/Groups/${groupId}/members/${userId}/role`, {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
        Authorization: `Bearer ${window.getToken()}`
      },
      body: JSON.stringify(role)
    });
    if (!res.ok) {
      const err = await res.json().catch(() => null);
      throw new Error(err?.message || `Change role failed: ${res.status}`);
    }
  },
  leave: async groupId => {
    const res = await fetch(`${window.apiBaseUrl}/Groups/${groupId}/leave`, {
      method: 'DELETE',
      headers: {
        Accept: 'application/json',
        Authorization: `Bearer ${window.getToken()}`
      }
    });
    if (!res.ok) {
      const err = await res.json().catch(() => null);
      throw new Error(err?.message || `Leave group failed: ${res.status}`);
    }
  },
  deleteGroup: async groupId => {
    const res = await fetch(`${window.apiBaseUrl}/Groups/${groupId}`, {
      method: 'DELETE',
      headers: {
        Accept: 'application/json',
        Authorization: `Bearer ${window.getToken()}`
      }
    });
    if (!res.ok) {
      const err = await res.json().catch(() => null);
      throw new Error(err?.message || `Delete failed: ${res.status}`);
    }
  }
};
