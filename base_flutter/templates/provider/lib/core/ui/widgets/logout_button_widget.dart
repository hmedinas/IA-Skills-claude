import 'package:__PackageName__/core/constants/app_contans.dart';
import 'package:__PackageName__/core/router/app_router.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:__PackageName__/core/common/provider/session_provider.dart';
import 'package:__PackageName__/features/auth/presentation/provider/auth_provider.dart';

class LogoutButtonWidget extends StatelessWidget {
  const LogoutButtonWidget({super.key});

  @override
  Widget build(BuildContext context) {
    // solo icono de salida

    return FloatingActionButton(
        heroTag: 'logout_circular_btn',
        onPressed:() => _showLogoutDialog(context),
        backgroundColor: Colors.redAccent,
        foregroundColor: Colors.white,
        elevation: 4,
        child: const Icon(Icons.logout_rounded, size: 28),
        shape: const CircleBorder(),

        );
        /*
    // icono con texto cerra session
    return FloatingActionButton.extended(
      heroTag: 'logout_btn', // Importante si usas varios FAB en la app
      onPressed: () => _showLogoutDialog(context),
      label: const Text(
        'CERRAR SESIÓN',
        style: TextStyle(fontWeight: FontWeight.bold, letterSpacing: 1),
      ),
      icon: const Icon(Icons.logout_rounded),
      backgroundColor: Colors.redAccent,
      foregroundColor: Colors.white,
      elevation: 4,
    );
    */
  }

  void _showLogoutDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
          title: const Row(
            children: [
              Icon(Icons.warning_amber_rounded, color: Colors.redAccent),
              SizedBox(width: 10),
              Text("Cerrar Sesión"),
            ],
          ),
          content: const Text("¿Estás seguro de que deseas salir? Deberás ingresar tus credenciales nuevamente."),
          actions: [
            // Botón Cancelar
            TextButton(
              onPressed: () => Navigator.pop(context), // Cierra el diálogo
              child: const Text("CANCELAR", style: TextStyle(color: Colors.grey)),
            ),
            // Botón Confirmar
            ElevatedButton(
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.redAccent,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              ),
              onPressed: () {
                Navigator.pop(context); // Cierra el diálogo
                context.read<SessionProvider>().logout();
                context.read<AuthProvider>().clear();
                Navigator.pushReplacementNamed(context, AppRoutesScreen.splash);
              },
              child: const Text("SÍ, SALIR", style: TextStyle(color: Colors.white)),
            ),
          ],
        );
      },
    );
  }
}