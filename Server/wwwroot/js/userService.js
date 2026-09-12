window.userService = {
  getAll: async () => {
    const res = await fetch(`${window.apiBaseUrl}/Users`, {
      headers: {
        Accept: 'application/json',
        Authorization: `Bearer ${window.getToken()}`
      }
    });
    if (!res.ok) {
      return [];
    }
    return res.json();
  }
};
