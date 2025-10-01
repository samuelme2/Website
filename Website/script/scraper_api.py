import requests
from bs4 import BeautifulSoup
import pandas as pd
import time
from urllib.parse import urljoin
import re
import os

PRODUCTO_URLS = [
     'https://distrisexcolombia.com/producto/dildo-vibrador-mission-2-lovense/', 
    'https://distrisexcolombia.com/producto/lovense-lush-2/',
    'https://distrisexcolombia.com/producto/lovense-lush-3/',
    'https://distrisexcolombia.com/producto/lovense-lush-4/',
    'https://distrisexcolombia.com/producto/lovense-lush-mini/',
    'https://distrisexcolombia.com/producto/lovense-nora/',
    'https://distrisexcolombia.com/producto/lovense-osci-3/',
    'https://distrisexcolombia.com/producto/vibrador-lapis-lovense/',
    'https://distrisexcolombia.com/producto/vibrador-vulse-lovense/',
    'https://distrisexcolombia.com/producto/succionador-tenera/',
    'https://distrisexcolombia.com/producto/lovense-domi-2-hitachi-tokens/',
    'https://distrisexcolombia.com/producto/lovense-hush-2/',
    'https://distrisexcolombia.com/producto/lovense-edge-2/',
    'https://distrisexcolombia.com/producto/lovense-ridge-anal/',
    'https://distrisexcolombia.com/producto/lovense-gush-2-masturbador-masculino/',
    'https://distrisexcolombia.com/producto/lovense-calor-masturbador-masculino/',
    'https://distrisexcolombia.com/producto/lovense-max-2-masturbador-masculino/',
    'https://distrisexcolombia.com/producto/camara-lovense-4k-webcam-2/',
    'https://distrisexcolombia.com/producto/pezoneras-vibratorias-gemini-lovense/',
    'https://distrisexcolombia.com/producto/sex-machine-lovense/',
    'https://distrisexcolombia.com/producto/sex-machine-lovense/',
    'https://distrisexcolombia.com/producto/lovense-mini-sex-machine/',
    'https://distrisexcolombia.com/producto/lovense-exomoon/',
    'https://distrisexcolombia.com/producto/dildo-vibrador-mission-2-lovense/',
    'https://distrisexcolombia.com/producto/vibrador-gravity-con-empuje-automatico-lovense/',
]

HEADERS = {
    'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
}

# Selector que apunta a los elementos <a> dentro de la galería de WooCommerce
TODAS_LAS_IMAGENES_SELECTOR = '.woocommerce-product-gallery__image a'
# Selector para los enlaces de categorías dentro de la meta del producto en WooCommerce
CATEGORIAS_SELECTOR = '.product_meta .posted_in a'

RUTA_BASE = r'C:\Users\USUARIO\Desktop\ScraperPython'
NOMBRE_ARCHIVO = 'productos_detallados_con_precio.csv'
RUTA_COMPLETA = os.path.join(RUTA_BASE, NOMBRE_ARCHIVO)


def limpiar_texto_html(html_content):
    if not html_content:
        return ''
    clean_text = html_content.get_text(separator=' ', strip=True)
    clean_text = re.sub(r'\s\s+', ' ', clean_text)
    return clean_text.strip()


def extraer_numeros(texto):
    # Extrae solo números del texto.
    # Ejemplo: "$ 1.250.000 COP" -> "1250000"
    numeros = re.findall(r'\d+', texto)
    if numeros:
        return int("".join(numeros))
    return 0


def obtener_datos_producto(url):
    print(f"  -> Accediendo a: {url}")
    try:
        response = requests.get(url, headers=HEADERS, timeout=15)
        response.raise_for_status()
        soup = BeautifulSoup(response.text, 'html.parser')

        nombre_element = soup.find('h1', class_='product_title')
        nombre = nombre_element.text.strip() if nombre_element else 'Nombre no encontrado'

        # ----------------------------------------------------------------------------------
        # --- NUEVA LÓGICA: EXTRACCIÓN DE CATEGORÍAS ---
        # ----------------------------------------------------------------------------------
        categorias_elementos = soup.select(CATEGORIAS_SELECTOR)
        categorias = [elem.text.strip() for elem in categorias_elementos]
        categorias_string = "|".join(categorias) if categorias else 'Categoría no encontrada'
        # ----------------------------------------------------------------------------------


        # --- LÓGICA DE IMÁGENES (sin cambios) ---
        imagenes = []
        
        # 1. Intentar obtener la imagen principal (miniatura)
        img_principal_element = soup.find('img', class_='wp-post-image')
        if img_principal_element:
            img_principal_url = img_principal_element.get('src')
            if img_principal_url and 'http' in img_principal_url:
                imagenes.append(img_principal_url)

        # 2. Recorrer los elementos de la galería y añadir las URLs que no estén ya
        galeria_elements = soup.select(TODAS_LAS_IMAGENES_SELECTOR)
        
        # Usamos un conjunto para URLs que ya hemos visto para evitar duplicados
        urls_vistas = set(imagenes) 
        
        for element in galeria_elements:
            img_url_href = element.get('href') 
            img_url_data = element.get('data-large_image')
            
            url_a_agregar = img_url_data if img_url_data else img_url_href
            
            if url_a_agregar and 'http' in url_a_agregar and url_a_agregar not in urls_vistas:
                imagenes.append(url_a_agregar)
                urls_vistas.add(url_a_agregar)


        imagenes_string = "|".join(imagenes)
        if not imagenes_string:
             imagenes_string = 'Ninguna imagen encontrada'

        # --- FIN LÓGICA DE IMÁGENES ---
        
        
        # --- LÓGICA DE DETALLES Y PRECIO (sin cambios) ---
        detalles_texto = ''
        detalles_element_corto = soup.find('div', class_='woocommerce-product-details__short-description')
        if detalles_element_corto:
            detalles_texto = limpiar_texto_html(detalles_element_corto)

        if not detalles_texto or len(detalles_texto) < 5:
            detalles_element_largo = soup.find('div', class_='woocommerce-Tabs-panel--description')
            if detalles_element_largo:
                detalles_texto = limpiar_texto_html(detalles_element_largo)
            elif soup.find('div', id='tab-description'):
                detalles_texto = limpiar_texto_html(soup.find('div', id='tab-description'))

        # Intentamos obtener el precio directamente si hay un selector mejor (ej. de woocommerce)
        precio_element = soup.select_one('.price .amount bdi')
        if precio_element:
             precio_texto = precio_element.text
        else:
            # Fallback a buscar en el resto del contenido
            precio_texto = detalles_texto 
            
        precio = extraer_numeros(precio_texto)

        descripcion_tab = soup.select_one('#tabs-list-description')
        descripcion_extendida = limpiar_texto_html(descripcion_tab) if descripcion_tab else "Descripción extendida no encontrada"
        # --- FIN LÓGICA DE DETALLES Y PRECIO ---


        return {
            'Nombre': nombre,
            'Precio': precio,
            'Categorias': categorias_string, # <--- ¡NUEVO CAMPO!
            'URLs_Imagenes': imagenes_string,
            'Descripcion_Extendida': descripcion_extendida,
            'URL_Producto': url
        }

    except requests.exceptions.RequestException as e:
        print(f"  -> ERROR al acceder a {url}: {e}")
        return {
            'Nombre': 'Error de Acceso',
            'Precio': 0,
            'Categorias': 'Error de Acceso', # <--- ¡NUEVO CAMPO!
            'URLs_Imagenes': 'Error de Acceso',
            'Descripcion_Extendida': 'Error de Acceso',
            'URL_Producto': url
        }
    except Exception as e:
        print(f"  -> ERROR de Scraper en {url}: {e}")
        return {
            'Nombre': 'Error Interno',
            'Precio': 0,
            'Categorias': 'Error Interno', # <--- ¡NUEVO CAMPO!
            'URLs_Imagenes': 'Error Interno',
            'Descripcion_Extendida': 'Error Interno',
            'URL_Producto': url
        }


if not PRODUCTO_URLS:
    print("Por favor, edita el script y añade las URLs de los productos que deseas raspar.")
else:
    datos_productos = []

    if not os.path.exists(RUTA_BASE):
        os.makedirs(RUTA_BASE)
        print(f"📁 Carpeta creada: {RUTA_BASE}")

    for i, url in enumerate(PRODUCTO_URLS):
        datos_productos.append(obtener_datos_producto(url))
        pausa = 1 + (i % 3)
        time.sleep(pausa)

    df = pd.DataFrame(datos_productos)
    df.to_csv(RUTA_COMPLETA, index=False, encoding='utf-8-sig')

    print("\n✅ Proceso completado.")
    print(f"Los datos se guardaron en: {RUTA_COMPLETA}")