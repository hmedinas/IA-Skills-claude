import 'package:__PackageName__/core/common/provider/session_provider.dart';
import 'package:__PackageName__/core/constants/app_contans.dart';
import 'package:__PackageName__/core/theme/app_theme.dart';
import 'package:__PackageName__/core/utils/alerts.dart';
import 'package:__PackageName__/features/auth/presentation/provider/auth_provider.dart';
import 'package:__PackageName__/features/auth/presentation/widgets/card_container_widget.dart';
import 'package:__PackageName__/features/auth/presentation/widgets/login_background_widget.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

class LoginScreen extends StatelessWidget {
  const LoginScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: LoginBackgroundWidget(
        child: Column(
          children: [
            SizedBox(height: 180),
            CardContainerWidget(
              child: Column(
                children: [
                  Container(
                    width: 230,
                    height: 60,
                    child: Image(
                      image: AssetImage(AppAssets.logo),
                      fit: BoxFit.fill,
                    ),
                  ),
                  SizedBox(height: 10),
                  Text('Inicio de sesión', style: AppTheme.initLoginTitle),
                  SizedBox(height: 15),
                  const _LoginForm(),
                  SizedBox(height: 15),
                  GestureDetector(
                    onTap: () {
                      Navigator.pushNamed(context, 'recuperarPass');
                    },
                    child: Text(
                      'Olvido Contraseña',
                      style: TextStyle(color: AppColors.titleGraphic),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _LoginForm extends StatelessWidget {
  const _LoginForm();

  @override
  Widget build(BuildContext context) {
    final authProvider = context.watch<AuthProvider>();

    return Container(
      child: Column(
        children: [
          SizedBox(height: 5),
          TextField(
            onChanged: authProvider.setUserName,
            decoration: const InputDecoration(
              labelText: 'Usuario',
              prefixIcon: Icon(Icons.account_circle),
            ),
          ),
          const SizedBox(height: 15),
          TextField(
            onChanged: authProvider.setPassword,
            obscureText: true,
            decoration: const InputDecoration(
              hintText: '********',
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
            onPressed: authProvider.isLoading
                ? null
                : () async {
                    // quitar el teclado
                    FocusScope.of(context).unfocus();

                    if (!authProvider.isValidForm()) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(
                          content: Text('Usuario o contraseña inválidos'),
                        ),
                      );
                      return;
                    }

                    // Llamar login
                    final response = await authProvider.login();

                    if (response == null)
                        return;

                    if(response.isAuthenticated){
                         mostrarAlert(context, "OK", typeAlert.ok);

                        // Guardamos los datos en el store global de sesión
                        context.read<SessionProvider>().updateSession(response);

                        Navigator.pushReplacementNamed(context, AppRoutesScreen.selectBusinessGrid);
                        return;
                    }
                    else{
                        mostrarAlert(context, response.message, typeAlert.error);
                        return;
                    }
                  },
            child: Container(
              padding: EdgeInsets.symmetric(horizontal: 80, vertical: 15),
              child: Text(
                authProvider.isLoading ? 'Espere ...' : 'Ingresar',
                style: TextStyle(color: Colors.white, fontSize: 18),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
