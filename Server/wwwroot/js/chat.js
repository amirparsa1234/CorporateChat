async function initChatPage() {
  if (!window.location.pathname.endsWith("chat.html")) return;
  const params    = new URLSearchParams(window.location.search);
  const otherId   = parseInt(params.get("userId"));
  const otherName = decodeURIComponent(params.get("username"));
  document.getElementById("chatWith").textContent = `Chat with ${otherName}`;
  const payload = JSON.parse(atob(window.getToken().split(".")[1]));
  const userId  = payload.sub || payload.UserId;
  const ul      = document.getElementById("chatMessages");

  try {
    const msgs = await chatService.getConversation(userId, otherId);
    msgs.forEach(m => {
      const sender = m.senderId === userId ? "Me" : otherName;
      const li     = document.createElement("li");
      li.textContent = `${sender}: ${m.content}`;
      ul.appendChild(li);
    });
  } catch (e) {
    console.error(e);
  }

  document.getElementById("chatForm").addEventListener("submit", async e => {
    e.preventDefault();
    const input = document.getElementById("chatInput");
    const txt   = input.value.trim();
    if (!txt) return;
    try {
      await connection.invoke("SendMessage", { receiverId: otherId, content: txt });
      input.value = "";
    } catch (err) {
      alert(err.message);
    }
  });
}

if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", initChatPage);
} else {
  initChatPage();
}
