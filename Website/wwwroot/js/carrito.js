// carrito.js — detecta imagen automáticamente desde la tarjeta del producto
// Reemplaza el archivo actual en wwwroot/js/carrito.js

// --- helpers ---
function guardarCarrito(carrito) {
    localStorage.setItem("carrito", JSON.stringify(carrito));
}

function cargarCarrito() {
    return JSON.parse(localStorage.getItem("carrito")) || [];
}

function parsePriceFromString(s) {
    if (!s) return 0;
    // quitar todo menos números, punto y coma/coma
    let cleaned = String(s).replace(/[^\d.,-]/g, "");
    if (cleaned === "") return 0;
    // casos: "12.345,67" -> remove dots, replace comma with dot
    if (cleaned.indexOf('.') > -1 && cleaned.indexOf(',') > -1) {
        cleaned = cleaned.replace(/\./g, '').replace(',', '.');
    } else if (cleaned.indexOf(',') > -1 && cleaned.indexOf('.') === -1) {
        // "12345,67" -> "12345.67"
        cleaned = cleaned.replace(',', '.');
    } else {
        // "12,345" or "12345" -> remove commas used as thousands
        cleaned = cleaned.replace(/,/g, '');
    }
    const n = parseFloat(cleaned);
    return isNaN(n) ? 0 : n;
}

function ensureAnimateCssLoaded() {
    if (!document.querySelector('link[data-animatecss]')) {
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = 'https://cdnjs.cloudflare.com/ajax/libs/animate.css/4.1.1/animate.min.css';
        link.setAttribute('data-animatecss', '1');
        document.head.appendChild(link);
    }
}

// --- lógica del carrito ---
let carrito = cargarCarrito();

function actualizarCarritoUI() {
    const cartCount = document.getElementById("cart-count");
    const itemsContainer = document.getElementById("cart-items");
    const totalElement = document.getElementById("cart-total");

    if (!itemsContainer) return; // offcanvas no está en la vista

    itemsContainer.innerHTML = "";
    let total = 0;
    let qtyTotal = 0;

    carrito.forEach((item, i) => {
        total += item.precio * item.cantidad;
        qtyTotal += item.cantidad;

        const li = document.createElement("li");
        li.className = "list-group-item d-flex justify-content-between align-items-center";

        li.innerHTML = `
            <div class="d-flex align-items-center gap-2" style="min-width:0;">
                <img src="${escapeHtml(item.imagen || '/images/no-image.png')}" alt="${escapeHtml(item.nombre)}"
                     style="width:56px; height:56px; object-fit:cover; border-radius:8px;">
                <div class="text-truncate" style="max-width:220px;">
                    <div class="fw-bold text-truncate">${escapeHtml(item.nombre)}</div>
                    <div class="small text-muted">$${(item.precio).toLocaleString()} c/u</div>
                </div>
            </div>
            <div class="d-flex align-items-center gap-2">
                <button class="btn btn-sm btn-outline-secondary restar-item" data-index="${i}" aria-label="Restar">−</button>
                <span class="badge bg-light text-dark px-2">${item.cantidad}</span>
                <button class="btn btn-sm btn-outline-secondary sumar-item" data-index="${i}" aria-label="Sumar">+</button>
                <button class="btn btn-sm btn-danger ms-2 eliminar-item" data-index="${i}" aria-label="Eliminar">x</button>
            </div>
        `;

        itemsContainer.appendChild(li);
    });

    if (totalElement) totalElement.textContent = `$${total.toLocaleString()}`;
    if (cartCount) {
        cartCount.textContent = qtyTotal;
        cartCount.style.display = qtyTotal > 0 ? "inline-block" : "none";
    }

    guardarCarrito(carrito);
}

function escapeHtml(str) {
    if (typeof str !== "string") return str;
    return str.replace(/[&<>"'`=\/]/g, function (s) {
        return ({
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            '"': "&quot;",
            "'": "&#39;",
            "/": "&#x2F;",
            "`": "&#x60;",
            "=": "&#x3D;"
        })[s];
    });
}

function obtenerInfoProductoDesdeBoton(btn) {
    const dataset = btn.dataset || {};
    let id = dataset.id || null;
    let nombre = dataset.nombre || null;
    let precio = dataset.precio || null;
    let imagen = dataset.imagen || null;

    const card = btn.closest('.card');

    if (!nombre && card) {
        const nameEl = card.querySelector('.card-title, h5');
        if (nameEl) nombre = nameEl.textContent.trim();
    }

    if (!precio && card) {
        const priceEl = card.querySelector('.producto-precio, .price, .card-text');
        if (priceEl) precio = parsePriceFromString(priceEl.textContent);
    }

    if (!imagen && card) {
        const imgEl = card.querySelector('img');
        if (imgEl) imagen = imgEl.src;
    }

    // 👉 Generar imagen automáticamente si no se encontró en la tarjeta
    if (!imagen && nombre) {
        let nombreLimpio = nombre
            .toLowerCase()                 // todo a minúscula
            .replace(/[^a-z0-9]+/g, '')    // eliminar espacios y símbolos
            .trim();
        imagen = `/images/${nombreLimpio}.jpg`;
    }

    if (typeof precio === 'string') precio = parsePriceFromString(precio);
    if (typeof precio !== 'number' || isNaN(precio)) precio = 0;

    return {
        id,
        nombre: nombre || 'Producto',
        precio,
        imagen: imagen || '/images/no-image.png'
    };
} function agregarAlCarritoDesdeBoton(btn) {
    const info = obtenerInfoProductoDesdeBoton(btn);
    // identificar por id si existe, sino por nombre
    let existente = null;
    if (info.id) {
        existente = carrito.find(it => it.id && String(it.id) === String(info.id));
    }
    if (!existente) {
        existente = carrito.find(it => it.nombre === info.nombre);
    }

    if (existente) {
        existente.cantidad++;
    } else {
        const item = {
            id: info.id || null,
            nombre: info.nombre,
            precio: Number(info.precio) || 0,
            cantidad: 1,
            imagen: info.imagen
        };
        carrito.push(item);
    }

    // animaciones
    animarFlyToCart(btn);
    animarIconoCarrito();

    actualizarCarritoUI();
}

function sumarUnidad(index) {
    index = Number(index);
    if (Number.isNaN(index) || !carrito[index]) return;
    carrito[index].cantidad++;
    actualizarCarritoUI();
}

function restarUnidad(index) {
    index = Number(index);
    if (Number.isNaN(index) || !carrito[index]) return;
    if (carrito[index].cantidad > 1) carrito[index].cantidad--;
    else carrito.splice(index, 1);
    actualizarCarritoUI();
}

function eliminarProducto(index) {
    index = Number(index);
    if (Number.isNaN(index) || !carrito[index]) return;
    carrito.splice(index, 1);
    actualizarCarritoUI();
}

function enviarWhatsApp() {
    if (!carrito || carrito.length === 0) {
        alert("Tu carrito está vacío.");
        return;
    }
    let mensaje = "🛒 Hola, quiero pedir:\n\n";
    let total = 0;
    carrito.forEach((it, i) => {
        mensaje += `${i + 1}. ${it.nombre} x${it.cantidad} - $${(it.precio * it.cantidad).toLocaleString()}\n`;
        total += it.precio * it.cantidad;
    });
    mensaje += `\n*TOTAL: $${total.toLocaleString()}*`;
    const telefono = "573248416899";
    window.open(`https://api.whatsapp.com/send?phone=${telefono}&text=${encodeURIComponent(mensaje)}`, '_blank');
}

// --- animación: icon + fly image ---
function animarIconoCarrito() {
    ensureAnimateCssLoaded();
    const icon = document.querySelector('.bi-cart3') || document.querySelector('#cart-count') || document.querySelector('[data-cart-icon]');
    if (!icon) return;
    icon.classList.remove('animate__animated', 'animate__tada');
    // trigger reflow to restart animation
    void icon.offsetWidth;
    icon.classList.add('animate__animated', 'animate__tada');
    setTimeout(() => {
        icon.classList.remove('animate__animated', 'animate__tada');
    }, 900);
}

function animarFlyToCart(btn) {
    const card = btn.closest('.card') || btn.closest('.producto-card') || btn.closest('.col') || document.body;
    const imgEl = card ? card.querySelector('img') : null;
    const cartIcon = document.querySelector('.bi-cart3') || document.querySelector('#cart-count') || document.querySelector('[data-cart-icon]');
    if (!imgEl || !cartIcon) return;

    const src = imgEl.src;
    const imgRect = imgEl.getBoundingClientRect();
    const cartRect = cartIcon.getBoundingClientRect();

    const clone = imgEl.cloneNode(true);
    clone.style.position = 'fixed';
    clone.style.left = imgRect.left + window.scrollX + 'px';
    clone.style.top = imgRect.top + window.scrollY + 'px';
    clone.style.width = imgRect.width + 'px';
    clone.style.height = imgRect.height + 'px';
    clone.style.objectFit = 'cover';
    clone.style.borderRadius = getComputedStyle(imgEl).borderRadius || '8px';
    clone.style.transition = 'all 650ms ease-in-out';
    clone.style.zIndex = 9999;
    document.body.appendChild(clone);

    // fuerza layout
    requestAnimationFrame(() => {
        clone.style.left = cartRect.left + window.scrollX + 'px';
        clone.style.top = cartRect.top + window.scrollY + 'px';
        clone.style.width = Math.max(16, cartRect.width) + 'px';
        clone.style.height = Math.max(16, cartRect.height) + 'px';
        clone.style.opacity = '0.2';
        clone.style.transform = 'scale(0.6)';
    });

    // cleanup
    setTimeout(() => clone.remove(), 700);
}

// --- inicialización y delegación ---
document.addEventListener("DOMContentLoaded", () => {
    console.log("✅ carrito.js (auto-img) cargado");
    carrito = cargarCarrito();
    actualizarCarritoUI();

    // Delegación global de clicks
    document.addEventListener('click', function (e) {
        const addBtn = e.target.closest && e.target.closest('.add-to-cart');
        if (addBtn) {
            e.preventDefault();
            agregarAlCarritoDesdeBoton(addBtn);
            return;
        }

        const sumar = e.target.closest && e.target.closest('.sumar-item');
        if (sumar) {
            e.preventDefault();
            sumarUnidad(sumar.dataset.index);
            return;
        }

        const restar = e.target.closest && e.target.closest('.restar-item');
        if (restar) {
            e.preventDefault();
            restarUnidad(restar.dataset.index);
            return;
        }

        const eliminar = e.target.closest && e.target.closest('.eliminar-item');
        if (eliminar) {
            e.preventDefault();
            eliminarProducto(eliminar.dataset.index);
            return;
        }
    });

    // conectado al botón final (si existe)
    const checkoutBtn = document.getElementById('checkout-btn');
    if (checkoutBtn) checkoutBtn.addEventListener('click', enviarWhatsApp);

    // Exponer global por si usas onclick inline
    window.enviarWhatsApp = enviarWhatsApp;
});
