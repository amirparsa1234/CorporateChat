document.addEventListener("DOMContentLoaded", function () {
  const form = document.getElementById("registerForm");

  if (!form) {
    return;
  }

  form.addEventListener("submit", async function (event) {
    event.preventDefault();

    const usernameInput = document.getElementById("username");
    const emailInput = document.getElementById("email");
    const passwordInput = document.getElementById("password");
    const submitButton = form.querySelector('button[type="submit"]');

    if (!usernameInput || !emailInput || !passwordInput) {
      alert("Registration form fields were not found");
      return;
    }

    const userName = usernameInput.value.trim();
    const email = emailInput.value.trim();
    const password = passwordInput.value;

    if (!userName || !email || !password) {
      alert("Please complete all fields");
      return;
    }

    if (submitButton) {
      submitButton.disabled = true;
    }

    try {
      await window.authService.register(
        userName,
        password,
        email
      );

      window.location.href = "index.html";
    } catch (error) {
      console.error(error);
      alert(error.message || "Registration error");
    } finally {
      if (submitButton) {
        submitButton.disabled = false;
      }
    }
  });
});
