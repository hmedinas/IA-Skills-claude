import 'package:__PackageName__/core/common/provider/session_provider.dart';
import 'package:__PackageName__/core/constants/app_contans.dart';
import 'package:__PackageName__/core/theme/app_theme.dart';
import 'package:__PackageName__/core/utils/alerts.dart';
import 'package:__PackageName__/features/auth/presentation/provider/auth_notifier.dart';
import 'package:__PackageName__/features/auth/presentation/widgets/login_background_widget.dart';
import 'package:__PackageName__/features/auth/presentation/widgets/card_container_widget.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class LoginScreen extends ConsumerWidget {
  const LoginScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Scaffold(
      body: LoginBackgroundWidget(
        child: Column(
          children: [
            const SizedBox(height: 180),
            CardContainerWidget(
              child: Column(
                children: [
                  const SizedBox(
                    width: 230,
                    height: 60,
                    child: Image(
                      image: AssetImage(AppAssets.logo),
                      fit: BoxFit.fill,
                    ),
                  ),
                  const SizedBox(height: 10),
                  Text(
                    'Inicio de sesión',
                    style: AppTheme.initLoginTitle,
                  ),
                  const SizedBox(height: 15),
                  const _LoginForm(),
                  const SizedBox(height: 15),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _LoginForm extends ConsumerWidget {
  const _LoginForm();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    // Escuchamos el ESTADO (como un computed de Vue)
    // esto escucha (userName, password, isLoading, errorMessage)
    final authState = ref.watch(authProvider);

    // Accedemos a la LÓGICA (los métodos)
    final authLogic = ref.read(authProvider.notifier);

    return Column(
      children: [
        TextField(
          onChanged: authLogic.setUserName,
          decoration: const InputDecoration(
            labelText: 'Usuario',
            prefixIcon: Icon(Icons.account_circle),
          ),
        ),
        const SizedBox(height: 15),
        TextField(
          onChanged: authLogic.setPassword,
          obscureText: true,
          decoration: const InputDecoration(
            labelText: 'Contraseña',
            prefixIcon: Icon(Icons.lock),
          ),
        ),
        const SizedBox(height: 25),
        ElevatedButton(
          style: ElevatedButton.styleFrom(
            backgroundColor: AppColors.buttonPrimary, // color normal
            foregroundColor: Colors.white, // texto
            disabledBackgroundColor:
                Colors.grey, // color cuando está deshabilitado
          ),
          onPressed: authState.isLoading
              ? null
              : () async {
                  // quitamos el teclado
                  FocusScope.of(context).unfocus();

                  if (!authLogic.isValidForm()) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(
                        content: Text('Usuario o contraseña inválidos'),
                      ),
                    );
                    return;
                  }

                  // Llamar login
                  final response = await authLogic.login();

                  if (response == null) return;

                  if (response.isAuthenticated) {
                    mostrarAlert(context, "OK", typeAlert.ok);

                    // Guardamos los datos en el store global de sesión
                    ref.read(sessionProvider.notifier).updateSession(response);

                    Navigator.pushReplacementNamed(
                      context,
                      AppRoutesScreen.selectBusinessGrid,
                    );
                  } else {
                    mostrarAlert(context, response.message, typeAlert.error);
                  }
                },
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 80, vertical: 15),
            child: Text(
              authState.isLoading ? 'Espere ...' : 'Ingresar',
              style: const TextStyle(color: Colors.white, fontSize: 18),
            ),
          ),
        ),
      ],
    );
  }
}
