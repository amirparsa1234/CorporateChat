document.addEventListener('DOMContentLoaded', () => {
  document.getElementById('loginForm')?.addEventListener('submit', async e => {
    e.preventDefault();
    const userName = document.getElementById('username').value.trim();
    const password = document.getElementById('password').value;
    try {
      await window.authService.login(userName, password);
      window.location.href = 'users.html';
    } catch (err) {
      alert(err.message || 'Login failed');
    }
  });
});
