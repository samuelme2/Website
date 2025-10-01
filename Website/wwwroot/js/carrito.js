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
    let cleaned = String(s).replace(/[^\d.,-]/g, "");
    if (cleaned === "") return 0;

    if (cleaned.indexOf('.') > -1 && cleaned.indexOf(',') > -1) {
        cleaned = cleaned.replace(/\./g, '').replace(',', '.');
    } else if (cleaned.indexOf(',') > -1 && cleaned.indexOf('.') === -1) {
        cleaned = cleaned.replace(',', '.');
    } else {
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

    if (!itemsContainer) return;

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

    if (!imagen && nombre) {
        let nombreLimpio = nombre.toLowerCase().replace(/[^a-z0-9]+/g, '').trim();
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
}

// --- agregar al carrito ---
function agregarAlCarritoDesdeBoton(btn) {
    const info = obtenerInfoProductoDesdeBoton(btn);

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
        carrito.push({
            id: info.id || null,
            nombre: info.nombre,
            precio: Number(info.precio) || 0,
            cantidad: 1,
            imagen: info.imagen
        });
    }

    // animaciones
    mostrarMensajeAgregado(btn);
    animarIconoCarrito();

    actualizarCarritoUI();
}

// --- sumar/restar/eliminar ---
function sumarUnidad(index) {
    index = Number(index);
    if (!carrito[index]) return;
    carrito[index].cantidad++;
    actualizarCarritoUI();
}

function restarUnidad(index) {
    index = Number(index);
    if (!carrito[index]) return;
    if (carrito[index].cantidad > 1) carrito[index].cantidad--;
    else carrito.splice(index, 1);
    actualizarCarritoUI();
}

function eliminarProducto(index) {
    index = Number(index);
    if (!carrito[index]) return;
    carrito.splice(index, 1);
    actualizarCarritoUI();
}

// --- WhatsApp ---
function enviarWhatsApp() {
    if (!carrito.length) {
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

// --- animaciones ---
function animarIconoCarrito() {
    ensureAnimateCssLoaded();
    const icon = document.querySelector('.bi-cart3') || document.querySelector('#cart-count');
    if (!icon) return;
    icon.classList.remove('animate__animated', 'animate__tada');
    void icon.offsetWidth;
    icon.classList.add('animate__animated', 'animate__tada');
    setTimeout(() => icon.classList.remove('animate__animated', 'animate__tada'), 900);
}

// 👉 NUEVO: mensaje “+1 agregado”
function mostrarMensajeAgregado(btn) {
    const msg = document.createElement('div');
    msg.textContent = "+1";
    msg.style.position = "absolute";
    msg.style.background = "rgba(0,0,0,0.8)";
    msg.style.color = "#fff";
    msg.style.padding = "2px 6px";
    msg.style.borderRadius = "4px";
    msg.style.fontSize = "0.8rem";
    msg.style.top = "-20px";
    msg.style.right = "0";
    msg.style.opacity = "1";
    msg.style.transition = "opacity 0.8s ease-out, transform 0.8s ease-out";
    msg.style.zIndex = "10000";

    const container = btn.parentElement || btn;
    container.style.position = "relative";
    container.appendChild(msg);

    requestAnimationFrame(() => {
        msg.style.opacity = "0";
        msg.style.transform = "translateY(-10px)";
    });

    setTimeout(() => msg.remove(), 900);
}

// --- inicialización ---
document.addEventListener("DOMContentLoaded", () => {
    console.log("✅ carrito.js (+1 agregado) cargado");
    carrito = cargarCarrito();
    actualizarCarritoUI();

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

    const checkoutBtn = document.getElementById('checkout-btn');
    if (checkoutBtn) checkoutBtn.addEventListener('click', enviarWhatsApp);

    window.enviarWhatsApp = enviarWhatsApp;
});
