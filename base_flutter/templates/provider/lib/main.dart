import 'package:__PackageName__/core/common/provider/app_state_provider.dart';
import 'package:__PackageName__/core/config/config_reader.dart';
import 'package:__PackageName__/core/router/app_router.dart';
import 'package:__PackageName__/core/theme/app_theme.dart';
import 'package:__PackageName__/core/utils/console.dart';
import 'package:flutter/material.dart';


void main() async {

    // 1. Inicializamos los WidgetsFlutterBinding, para acceder a los assets antes de arrancar la aplicacion. 
    WidgetsFlutterBinding.ensureInitialized();

    // 2. Iniciando la configuración según el entorno (inyectado por --dart-define)
    const env = String.fromEnvironment(
        'ENVIRONMENT',
        defaultValue: 'dev',
    );

    // Inicializamos el ConfigReader con el entorno deseado (se lee el JSON de assets)
    try
    {
        await ConfigReader.initialize(environment: env);
        await Console.initialize();
    } 
    catch (e) {
        debugPrint('Error crítico inicializando configuración: $e');
    }
  
    runApp(
        AppStateProvider(
            child: MainApp(environment: env),
        ),
    );
}

class MainApp extends StatelessWidget {
    final String environment;
    const MainApp({super.key, required this.environment});

    @override
    Widget build(BuildContext context) {
        return MaterialApp(
            debugShowCheckedModeBanner: false,
            title: '__AppDisplayName__',
            theme: AppTheme.baseTheme,
            initialRoute: AppRouter.initialRoute(environment),
            onGenerateRoute: (settings) => AppRouter.onGenerateRoute(settings, environment),
        );
    }
}
