
import 'package:__PackageName__/core/common/provider/session_provider.dart';
import 'package:__PackageName__/features/auth/data/sources/auth_api.dart';
import 'package:__PackageName__/features/auth/presentation/provider/auth_provider.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

class AppStateProvider extends StatelessWidget {
  final Widget child;
  const AppStateProvider({super.key, required this.child});

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => SessionProvider()),
        ChangeNotifierProvider(create: (_) => AuthProvider(AuthApi())),
        // Aquí puedes agregar otros providers
        // ChangeNotifierProvider(create: (_) => ResumenGeneralService()),
        // ChangeNotifierProvider(create: (_) => RecepcionVueloService()),
      ],
      child: child,
    );
  }
}
