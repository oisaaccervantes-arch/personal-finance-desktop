# Personal Finance

Empecé esta aplicación *vibecodeando* porque quería llevar un mejor control de mis gastos. Necesitaba ver cuánto dinero tenía en cada cuenta, registrar movimientos sin perder tiempo y entender qué pagos se acercaban. El proyecto fue creciendo hasta convertirse en una app de escritorio para organizar mis finanzas personales en un solo lugar.

Está hecha para Windows y funciona completamente en el equipo: los datos se guardan en SQLite, sin conexión bancaria, cuentas en la nube ni telemetría.

## Qué puedes hacer

- Ver el balance total, ingresos, gastos, ahorro del mes y movimientos recientes en el dashboard.
- Crear cuentas bancarias, de ahorro y tarjetas con banco, texto, color y saldo inicial propios. La cuenta **Efectivo** se crea automáticamente y aparece primero.
- Registrar, buscar, filtrar, editar y eliminar ingresos o gastos.
- Retirar efectivo de una cuenta o depositarlo en ella. Estas operaciones mueven saldo entre cuentas sin contarse como ingresos o gastos.
- Organizar movimientos con categorías y presupuestos mensuales.
- Programar cobros y pagos mensuales en un día elegido, quincenales (día 15 y último del mes) o catorcenales (cada 14 días desde una fecha inicial). Al abrir la app se registran los vencimientos pendientes una sola vez. Un pago ya registrado se puede editar, incluso para cambiar la cuenta con la que se pagó.
- Consultar una proyección de saldo para los próximos seis cierres de mes, basada en las recurrencias activas.
- Revisar reportes, exportar movimientos a CSV y crear o restaurar respaldos locales.
- Elegir un tema o personalizar la apariencia.

La interfaz está inspirada en **liquid glass**: superficies translúcidas, reflejos suaves, movimiento de fondo y tarjetas personalizables. La fuente Space Grotesk se incluye con su licencia OFL.

Los iconos de categorías usan [Bootstrap Icons](https://icons.getbootstrap.com/), tomados del [archivo de Figma de la comunidad](https://www.figma.com/design/mjtW5vqUQTfQaoPs2gMsAN/Bootstrap-Icons--Community-?node-id=0-1) e incluidos localmente bajo [licencia MIT](PersonalFinance/Assets/BootstrapIcons/LICENSE). La aplicación no los descarga al ejecutarse.

## Ejecutar desde el repositorio

Necesitas Windows y el SDK de .NET 10. Desde la carpeta del proyecto:

```powershell
dotnet run --project PersonalFinance/PersonalFinance.csproj -c Release
```

Para generar un ejecutable independiente para Windows x64:

```powershell
dotnet publish PersonalFinance/PersonalFinance.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist
```

Después abre `dist/PersonalFinance.exe`. La carpeta `dist` se genera localmente y no forma parte del código del repositorio.

## Primeros pasos

1. Abre **Cuentas**. Efectivo ya estará creado; añade tus cuentas o tarjetas e indica sus saldos iniciales.
2. Usa **Registrar movimiento** para un ingreso o gasto. En **Mover efectivo** puedes registrar retiros y depósitos entre Efectivo y otra cuenta.
3. Crea tus categorías y presupuestos. Si tienes pagos o ingresos fijos, agrégalos en **Recurrentes**.
4. Revisa los resultados en **Dashboard** y **Reportes**. En **Configuración** puedes guardar un respaldo y ajustar la apariencia.

La moneda de esta versión es **MXN**. En tarjetas de crédito se muestra el saldo registrado: la app todavía no administra límites de crédito ni calcula crédito disponible.

## Privacidad y respaldos

La base se guarda en `%LOCALAPPDATA%\PersonalFinance\finance.db`. No se almacenan números de tarjeta, contraseñas bancarias ni datos financieros en servidores externos. Los respaldos SQLite contienen los datos completos **sin cifrar**; guárdalos en una ubicación segura. La exportación CSV protege los campos de texto frente a fórmulas de hoja de cálculo.

La app conserva una copia automática antes de actualizar el esquema de una base existente y antes de restaurar un respaldo. Para pruebas, la variable `PERSONAL_FINANCE_DATA_DIR` permite usar otra carpeta sin tocar la base personal.

## Tecnología

| Capa | Tecnología |
| --- | --- |
| Interfaz | C#, .NET 10, WPF |
| Datos | SQLite, Entity Framework Core |
| Organización | MVVM, servicios de persistencia y controles reutilizables |

Los importes se almacenan como centavos enteros para evitar errores de redondeo. La app mantiene una sola cuenta Efectivo. Las transferencias no alteran el patrimonio total. Los movimientos recurrentes se generan por fecha de vencimiento y la proyección solo incluye recurrencias activas; no intenta predecir gastos ocasionales.

Para verificar el proyecto:

```powershell
dotnet build PersonalFinance.slnx -c Release
dotnet run --project PersonalFinance.Tests -c Release
dotnet run --project PersonalFinance.Tests -c Release -- --scroll
```

Las pruebas usan una base temporal independiente. El código de la app está en `PersonalFinance/` y las pruebas en `PersonalFinance.Tests/`. Los archivos de referencia de Stitch se conservan solo en el equipo de desarrollo y no forman parte del repositorio.

## Estado del proyecto

Este proyecto nació como una herramienta personal y sigue evolucionando. Está pensado para llevar un registro local y claro de las finanzas diarias; por ahora no ofrece sincronización entre equipos ni conexión automática con bancos.
