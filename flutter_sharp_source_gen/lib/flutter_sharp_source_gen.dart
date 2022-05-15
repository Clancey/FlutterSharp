library flutter_sharp_source_gen.dart;

import 'package:build/build.dart';
import 'src/flutter_sharp_source_gen_base.dart';

Builder flutter_sharpGeneratorFactory(BuilderOptions options) =>
    flutter_sharpGeneratorFactoryBuilder(
        header: options.config['header'] as String?);
