window.apiBaseUrl =
  window.apiBaseUrl ||
  window.CORPORATE_CHAT_CONFIG?.apiBaseUrl ||
  "/api";

function formatMessageDate(dateValue) {
  try {
    if (!dateValue) return new Date().toLocaleString(undefined, { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });
    let dv = dateValue;
    if (typeof dv === 'string' && !dv.endsWith('Z') && !dv.includes('+')) dv += 'Z'; 
    const d = new Date(dv);
    if (isNaN(d.getTime()) || d.getFullYear() < 2000) return new Date().toLocaleString(undefined, { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });
    return d.toLocaleString(undefined, { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });
  } catch {
    return new Date().toLocaleString(undefined, { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });
  }
}

window.addEventListener('DOMContentLoaded', async () => {
  const token = window.getToken();
  if (!token && !location.pathname.endsWith('login.html') && !location.pathname.endsWith('register.html')) {
    return location.href = 'login.html';
  }
  const payload = () => JSON.parse(atob(token.split('.')[1]));
  const userId = payload().sub || payload().UserId;
  const userName = payload().unique_name || payload().name;

  const currentUserSpan = document.getElementById('currentUser');
  const logoutBtn = document.getElementById('logoutBtn');
  if (currentUserSpan) currentUserSpan.textContent = `User: ${userName}`;
  if (logoutBtn) logoutBtn.addEventListener('click', () => authService.logout());

  const chatHeader = document.getElementById('chatHeader');
  if (!chatHeader) return;

  const ulUsers = document.getElementById('usersSection');
  if (ulUsers) {
    (await userService.getAll())
      .filter(u => u.id.toString() !== userId.toString())
      .forEach(u => {
        const li = document.createElement('li');
        li.textContent = u.userName;
        li.dataset.userId = u.id;
        ulUsers.appendChild(li);
      });
    ulUsers.classList.remove('hidden');
  }

  const ulGroups = document.getElementById('groupsSection');
  if (ulGroups) {
    let groups = [];
    try {
      groups = await groupService.getMy();
      if (groups.length) {
        groups.forEach(g => {
          const li = document.createElement('li');
          li.textContent = g.name;
          li.dataset.groupId = g.id;
          ulGroups.appendChild(li);
        });
      } else {
        document.getElementById('noGroupsMessage').classList.remove('hidden');
      }
    } catch {
      const noMsg = document.getElementById('noGroupsMessage');
      noMsg.textContent = 'Failed to load groups.';
      noMsg.classList.remove('hidden');
    }
    ulGroups.classList.remove('hidden');
  }

  document.getElementById('createGroupBtn')?.addEventListener('click', async () => {
    const name = prompt('Enter group name:')?.trim();
    if (!name) return;
    await groupService.create(name);
    const groups = await groupService.getMy();
    const ul = document.getElementById('groupsSection');
    ul.innerHTML = '';
    if (groups.length) {
      document.getElementById('noGroupsMessage').classList.add('hidden');
      groups.forEach(g => {
        const li = document.createElement('li');
        li.textContent = g.name;
        li.dataset.groupId = g.id;
        ul.appendChild(li);
      });
    } else {
      document.getElementById('noGroupsMessage').classList.remove('hidden');
    }
  });

  const ms = document.getElementById('chatMessages');
  const clearMessages = () => { ms.innerHTML = ''; };
  
  const renderMessage = m => {
    const senderId = m.senderId || m.SenderId;
    const isSent = senderId.toString() === userId.toString();
    const li = document.createElement('li');
    li.classList.add(isSent ? 'sent' : 'received');
    
    if (m.content || m.Content) {
        const textDiv = document.createElement('div');
        textDiv.textContent = m.content || m.Content;
        textDiv.style.marginBottom = '5px';
        li.appendChild(textDiv);
    }

    const attachments = m.attachments || m.Attachments || [];
    if (attachments.length > 0) {
        attachments.forEach(att => {
            const attDiv = document.createElement('div');
            attDiv.style.marginTop = '5px';
            const url = window.apiBaseUrl.replace('/api', '') + (att.fileUrl || att.FileUrl);
            const contentType = att.contentType || att.ContentType || '';
            
            if (contentType.startsWith('image/')) {
                const img = document.createElement('img');
                img.src = url;
                img.style.maxWidth = '200px';
                img.style.borderRadius = '8px';
                attDiv.appendChild(img);
            } else {
                const link = document.createElement('a');
                link.href = url;
                link.target = '_blank';
                link.textContent = '📎 ' + (att.fileName || att.FileName);
                link.style.color = isSent ? '#fff' : '#007bff';
                link.style.textDecoration = 'underline';
                attDiv.appendChild(link);
            }
            li.appendChild(attDiv);
        });
    }
    
    const dateValue = m.sentAt || m.SentAt || m.timestamp || m.Timestamp;
    const tsDiv = document.createElement('div');
    tsDiv.textContent = formatMessageDate(dateValue);
    tsDiv.style.fontSize = '0.75em';
    tsDiv.style.color = '#888';
    tsDiv.style.textAlign = 'right';

    li.appendChild(tsDiv);
    ms.appendChild(li);
    ms.scrollTop = ms.scrollHeight;
  };

  let current = null;
  const groupActions = document.getElementById('groupActions');
  const addMemberSection = document.getElementById('addMemberSection');

  const getRoleString = (r) => {
    if (r === 0 || r === 'Owner') return 'Owner';
    if (r === 1 || r === 'Admin') return 'Admin';
    return 'Member';
  };

  const renderGroupActions = async group => {
    if (!group || !Array.isArray(group.members)) return;
    const memberList = document.getElementById('memberList');
    memberList.innerHTML = '';
    
    const me = group.members.find(m => m.userId.toString() === userId.toString());
    if (!me) return;

    const myRole = getRoleString(me.role);
    const isOwner = myRole === 'Owner';
    const isAdmin = myRole === 'Admin';

    group.members.forEach(m => {
      const li = document.createElement('li');
      const mRole = getRoleString(m.role);
      li.textContent = `${m.user.userName} (${mRole})`;
      
      if (m.userId.toString() !== userId.toString() && (isOwner || (isAdmin && mRole === 'Member'))) {
        if (isOwner) {
          const sel = document.createElement('select');
          ['Member', 'Admin'].forEach(r => {
            const opt = document.createElement('option');
            opt.value = r === 'Member' ? 2 : 1;
            opt.textContent = r;
            if (r === mRole) opt.selected = true;
            sel.appendChild(opt);
          });
          sel.addEventListener('change', async () => {
            await groupService.changeRole(current.id, m.userId, parseInt(sel.value));
          });
          li.appendChild(sel);
        }
        const btn = document.createElement('button');
        btn.textContent = 'Remove';
        btn.classList.add('btn');
        btn.addEventListener('click', async () => {
          await groupService.removeMember(current.id, m.userId);
          const updated = await groupService.getById(current.id);
          renderGroupActions(updated);
        });
        li.appendChild(btn);
      }
      memberList.appendChild(li);
    });

    if (isOwner || isAdmin) {
      addMemberSection.classList.remove('hidden');
      const select = document.getElementById('newMemberSelect');
      select.innerHTML = '<option disabled selected value="">Select a user</option>';
      (await userService.getAll())
        .filter(u => u.id.toString() !== userId.toString() && !group.members.some(m => m.userId.toString() === u.id.toString()))
        .forEach(u => {
          const opt = document.createElement('option');
          opt.value = u.id;
          opt.textContent = u.userName;
          select.appendChild(opt);
        });
        
      const newRole = document.getElementById('newMemberRole');
      newRole.innerHTML = '<option value="2">Member</option><option value="1">Admin</option>';
      if (isAdmin) newRole.querySelector('option[value="1"]').remove();

      document.getElementById('addMemberBtn').onclick = async () => {
        const uid = parseInt(select.value);
        const role = parseInt(newRole.value);
        if (!uid) return;
        await groupService.addMember(current.id, uid, role);
        const updated = await groupService.getById(current.id);
        renderGroupActions(updated);
      };
    } else {
      addMemberSection.classList.add('hidden');
    }

    const leaveBtn = document.getElementById('leaveGroupBtn');
    leaveBtn.classList.remove('hidden');
    leaveBtn.textContent = isOwner ? 'Delete Group' : 'Leave Group';
    
    leaveBtn.onclick = async () => {
      await groupService.leave(current.id);
      groupActions.classList.add('hidden');
      document.getElementById('toggleGroupSettingsBtn').classList.add('hidden');
      chatHeader.textContent = 'Select a user or group';
      clearMessages();
      current = null;
      
      const ul = document.getElementById('groupsSection');
      ul.innerHTML = '';
      const groups = await groupService.getMy();
      if (groups.length) {
        document.getElementById('noGroupsMessage').classList.add('hidden');
        groups.forEach(g => {
          const li = document.createElement('li');
          li.textContent = g.name;
          li.dataset.groupId = g.id;
          ul.appendChild(li);
        });
      } else {
        document.getElementById('noGroupsMessage').classList.remove('hidden');
      }
    };
  };

  const loadChat = async () => {
    clearMessages();
    if (!current) return;
    chatHeader.textContent = current.name;
    if (current.type === 'user') {
      groupActions.classList.add('hidden');
      addMemberSection.classList.add('hidden');
      const msgs = await chatService.getWithUser(current.id);
      msgs.forEach(renderMessage);
    } else {
      const msgs = await chatService.getGroupMessages(current.id);
      await window.connection.invoke('JoinGroup', parseInt(current.id));
      msgs.forEach(renderMessage);
      const group = await groupService.getById(current.id);
      renderGroupActions(group);
    }
  };

  document.querySelectorAll('.toggle-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      document.getElementById(btn.dataset.target).classList.toggle('hidden');
    });
  });

  document.getElementById('usersSection')?.addEventListener('click', e => {
    if (e.target.tagName === 'LI') {
      current = { type: 'user', id: e.target.dataset.userId, name: e.target.textContent };
      loadChat();
    }
  });

  document.getElementById('groupsSection')?.addEventListener('click', e => {
    if (e.target.tagName === 'LI') {
      current = { type: 'group', id: e.target.dataset.groupId, name: e.target.textContent };
      loadChat();
    }
  });

  document.getElementById('chatForm')?.addEventListener('submit', async e => {
    e.preventDefault();
    const txtInput = document.getElementById('chatInput');
    const fileInput = document.getElementById('fileInput');
    const txt = txtInput.value.trim();
    const file = fileInput.files[0];
    
    if ((!txt && !file) || !current) return;

    let uploadResult = null;
    if (file) {
      const formData = new FormData();
      formData.append('file', file);
      const res = await fetch(`${window.apiBaseUrl}/files/upload`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${window.getToken()}` },
        body: formData
      });
      if (res.ok) {
        uploadResult = await res.json();
      } else {
        alert('File upload failed!');
        return;
      }
    }

    const messageDto = {
        content: txt,
        fileUrl: uploadResult ? uploadResult.url : null,
        fileName: uploadResult ? uploadResult.fileName : null,
        contentType: uploadResult ? uploadResult.contentType : null
    };

    if (current.type === 'user') {
      messageDto.receiverId = parseInt(current.id);
      await window.connection.invoke('SendMessage', messageDto);
    } else {
      messageDto.groupId = parseInt(current.id);
      await window.connection.invoke('SendMessage', messageDto);
    }
    
    txtInput.value = '';
    fileInput.value = '';
  });

  if (!window.connection) {
    window.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${window.location.origin}/hubs/chat`, { accessTokenFactory: () => window.getToken() })
      .withAutomaticReconnect()
      .build();

    window.connection.on('ReceiveMessage', renderMessage);
    window.connection.on('ReceiveGroupMessage', renderMessage);

    window.connection.start().catch(() => {});
  }
});
