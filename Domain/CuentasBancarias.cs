using System;
using System.Collections.Generic;
using System.Linq;

namespace Dsw2025Ej8.Domain
{
    // 1. Excepciones personalizadas
    public class MontoNoValidoException : Exception
    {
        public MontoNoValidoException()
            : base("El monto ingresado no es válido para la operación solicitada") { }
    }

    public class CuentaNoActivaException : Exception
    {
        public CuentaNoActivaException(string estado)
            : base($"No se puede operar con la cuenta {estado}") { }
    }

    public class SaldoInsuficienteException : Exception
    {
        public SaldoInsuficienteException()
            : base("La cuenta no cuenta con saldo para la operación solicitada. Fue suspendida.") { }
    }

    // 2. Clase base abstracta Cuenta con propiedades y constructor
    public abstract class Cuenta
    {
        public int Numero { get; }
        public decimal Saldo { get; protected set; }
        public bool Activa { get; protected set; } = true;

        protected Cuenta(int numero, decimal saldoInicial)
        {
            Numero = numero;
            Saldo = saldoInicial;
        }

        // Validaciones comunes
        protected void ValidarMonto(decimal monto)
        {
            if (monto <= 0)
                throw new MontoNoValidoException();
        }

        protected void ValidarActiva()
        {
            if (!Activa)
                throw new CuentaNoActivaException(Activa ? "Activa" : "Suspendida");
        }

        public abstract void Depositar(decimal monto);
        public abstract void Retirar(decimal monto);
    }

    // 3. Derivada CajaDeAhorro
    public class CajaDeAhorro : Cuenta
    {
        // Tasa de interés asignada tras crear la cuenta
        public decimal TasaInteres { get; set; }

        public CajaDeAhorro(int numero, decimal saldoInicial)
            : base(numero, saldoInicial) { }

        public override void Depositar(decimal monto)
        {
            ValidarActiva();
            ValidarMonto(monto);
            Saldo += monto;
        }

        public override void Retirar(decimal monto)
        {
            ValidarActiva();
            ValidarMonto(monto);
            if (Saldo < monto)
            {
                Activa = false;
                throw new SaldoInsuficienteException();
            }
            Saldo -= monto;
        }

        // Método para aplicar interés (puede llamarse externamente)
        public void AplicarInteres()
        {
            ValidarActiva();
            Saldo += Saldo * TasaInteres;
        }
    }

    // 4. Derivada CuentaCorriente
    public class CuentaCorriente : Cuenta
    {
        // Límite de descubierto asignado tras crear la cuenta
        public decimal LimiteDescubierto { get; set; }

        public CuentaCorriente(int numero, decimal saldoInicial)
            : base(numero, saldoInicial) { }

        public override void Depositar(decimal monto)
        {
            ValidarActiva();
            ValidarMonto(monto);
            Saldo += monto;
        }

        public override void Retirar(decimal monto)
        {
            ValidarActiva();
            ValidarMonto(monto);
            if (Saldo + LimiteDescubierto < monto)
            {
                Activa = false;
                throw new SaldoInsuficienteException();
            }
            Saldo -= monto;
        }
    }

    // 5. Programa de prueba
    class Program
    {
        static void Main(string[] args)
        {
            var cuentas = new List<Cuenta>();

            // Instanciar 4 cuentas
            var ahorro1 = new CajaDeAhorro(1001, 500m) { TasaInteres = 0.05m };
            var ahorro2 = new CajaDeAhorro(1002, 1000m) { TasaInteres = 0.03m };
            var corriente1 = new CuentaCorriente(2001, 200m) { LimiteDescubierto = 100m };
            var corriente2 = new CuentaCorriente(2002, 0m) { LimiteDescubierto = 50m };

            cuentas.AddRange(new Cuenta[] { ahorro1, ahorro2, corriente1, corriente2 });

            // Operaciones de prueba con manejo de excepciones
            void Ejecutar(Action accion, string descripcion)
            {
                try {
                    accion();
                    Console.WriteLine($"OK: {descripcion}");
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error ({descripcion}): {ex.Message}");
                }
            }

            // Depositos válidos e inválidos
            Ejecutar(() => ahorro1.Depositar(200m), "Depósito 200 en ahorro1");
            Ejecutar(() => ahorro1.Depositar(0m), "Depósito 0 en ahorro1 (MontoNoValido)");

            // Retiros válidos e insuficientes
            Ejecutar(() => ahorro2.Retirar(300m), "Retiro 300 en ahorro2");
            Ejecutar(() => ahorro2.Retirar(800m), "Retiro 800 en ahorro2 (SaldoInsuficiente)");

            // Intentar operar sobre cuenta suspendida
            Ejecutar(() => ahorro2.Depositar(100m), "Depósito 100 en ahorro2 suspendida (CuentaNoActiva)");

            // Aplicar interés en cuenta activa
            Ejecutar(() => ahorro1.AplicarInteres(), "Aplicar interés en ahorro1");

            // Operaciones en corriente con descubierto
            Ejecutar(() => corriente1.Retirar(250m), "Retiro 250 en corriente1 (dentro de descubierto)");
            Ejecutar(() => corriente1.Retirar(100m), "Retiro 100 en corriente1 (excede descubierto)");

            // Manejo de monto inválido en corriente
            Ejecutar(() => corriente2.Depositar(-50m), "Depósito -50 en corriente2 (MontoNoValido)");

            // Resumen final
            Console.WriteLine("\n--- Resumen de Cuentas ---");
            var resumen = cuentas.Select(c => new { c.Numero, Tipo = c.GetType().Name, c.Saldo });
            foreach (var r in resumen)
                Console.WriteLine($"Cuenta {r.Numero} ({r.Tipo}): Saldo={r.Saldo}");

        }
    }
}