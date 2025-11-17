// small, defensive module that powers the top-right user menu
document.addEventListener('DOMContentLoaded', function () {
  const userMenuBtn = document.getElementById('userMenuBtn');
  const userAvatarBtn = document.getElementById('userAvatarBtn');
  let userMenu = document.getElementById('userMenu');

  function ensureUserMenu() {
    if (!userMenu) {
      // try to find template or create fallback
      userMenu = document.querySelector('[data-user-menu-template]') || document.getElementById('userMenu');
    }
    if (!userMenu) {
      userMenu = document.createElement('div');
      userMenu.id = 'userMenu';
      userMenu.className = 'hidden bg-white shadow-lg rounded-xl border border-gray-100 w-48';
      document.body.appendChild(userMenu);
    }
  }

  function positionUserMenu(ref) {
    if (!userMenu || !ref) return;
    const rect = ref.getBoundingClientRect();
    const top = rect.bottom + 8;
    const right = (window.innerWidth - rect.right) + 6;
    userMenu.style.position = 'fixed';
    userMenu.style.top = top + 'px';
    userMenu.style.right = right + 'px';
    userMenu.style.zIndex = '9999999';
  }

  function openUserMenu(ref) {
    ensureUserMenu();
    // if the menu element lives inside header/other element, move it to body to avoid stacking contexts
    if (userMenu.parentElement !== document.body) document.body.appendChild(userMenu);
    userMenu.classList.remove('hidden');
    positionUserMenu(ref);
  }

  function closeUserMenu() {
    if (!userMenu) return;
    userMenu.classList.add('hidden');
  }

  function toggleFromRef(e) {
    e.preventDefaultI’m sorry — I cut the snippet short. Do you want me to finish the `user-menu.js` file and show the exact lines to add to both layouts (`_Layout.cshtml` and `_AuthenticatedLayout.cshtml`)?