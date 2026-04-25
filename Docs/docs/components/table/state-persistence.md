# Persistencia de estado

El componente puede guardar el estado de la tabla en `localStorage`.

## Activar persistencia

```razor
<Table PersistState="true"
       StateKey="users-table">
```

## Qué se guarda

La persistencia puede guardar:

```txt
Búsqueda global
Página actual
Tamaño de página
Filtros por columna
SearchPanes seleccionados
Búsquedas dentro de SearchPanes
Columnas ocultas
Ordenación
Estado de SearchPanes abierto/cerrado
```

## StateKey

`StateKey` debe ser único por tabla.

Ejemplos:

```razor
StateKey="users-table"
StateKey="orders-table"
StateKey="audit-log-table"
```

## Cuándo cambiar StateKey

Cambiar `StateKey` cuando:

- cambian las columnas;
- cambian claves `Key`;
- cambian valores por defecto;
- cambian SearchPanes;
- se quiere resetear la configuración del usuario;
- se detecta un estado incompatible.

Ejemplo:

```razor
StateKey="users-table-v2"
```

## Interacción con ShowByDefault

Si `PersistState="true"`, el estado guardado tiene prioridad sobre `ShowByDefault`.

Ejemplo:

```razor
<TableColumn TItem="User"
             Key="email"
             Title="Email"
             Value="@(u => u.Email)"
             ShowByDefault="false" />
```

Si el usuario mostró esa columna y el estado quedó guardado, la columna puede aparecer visible en futuras cargas.

## Interacción con SearchPanesInitialCollapsed

Si `PersistState="true"`, el estado guardado puede prevalecer sobre:

```razor
SearchPanesInitialCollapsed="true"
```

Para reiniciar:

```razor
StateKey="users-table-v2"
```

## JavaScript interop

La persistencia usa:

```txt
wwwroot/js/tableInterop.js
```

Funciones utilizadas:

```txt
getLocalState
setLocalState
removeLocalState
```

## Recomendaciones

- Usar `PersistState` solo cuando aporte valor real al usuario.
- Usar `StateKey` único por tabla.
- Versionar `StateKey` cuando cambie la estructura.
- No guardar datos sensibles en estado local.
- No confiar en el estado persistido para seguridad.