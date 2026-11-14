from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "CX Audio" / "Assets"
SOURCE = Image.open(ASSETS / "logo.png").convert("RGBA")


def square_asset(name: str, size: int, fill: float = 0.9) -> None:
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    edge = max(1, round(size * fill))
    icon = SOURCE.resize((edge, edge), Image.Resampling.LANCZOS)
    canvas.alpha_composite(icon, ((size - edge) // 2, (size - edge) // 2))
    canvas.save(ASSETS / name, format="PNG", optimize=True)


def wide_asset(name: str, width: int, height: int, icon_ratio: float) -> None:
    canvas = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    edge = max(1, round(min(width, height) * icon_ratio))
    icon = SOURCE.resize((edge, edge), Image.Resampling.LANCZOS)
    canvas.alpha_composite(icon, ((width - edge) // 2, (height - edge) // 2))
    canvas.save(ASSETS / name, format="PNG", optimize=True)


# Base assets referenced by Package.appxmanifest.
square_asset("Square150x150Logo.png", 150)
square_asset("Square44x44Logo.png", 44)
wide_asset("Wide310x150Logo.png", 310, 150, 0.78)
wide_asset("SplashScreen.png", 620, 300, 0.58)
square_asset("StoreLogo.png", 50)
square_asset("LockScreenLogo.png", 24)
square_asset("Square44x44Logo.targetsize-24_altform-unplated.png", 24)

# Scale-qualified assets are retained for Windows resource selection.
square_asset("Square150x150Logo.scale-200.png", 300)
square_asset("Square44x44Logo.scale-200.png", 88)
wide_asset("Wide310x150Logo.scale-200.png", 620, 300, 0.78)
wide_asset("SplashScreen.scale-200.png", 1240, 600, 0.58)
square_asset("LockScreenLogo.scale-200.png", 48)

# Keep the executable icon populated with the same artwork at common shell sizes.
SOURCE.save(
    ASSETS / "app.ico",
    format="ICO",
    sizes=[(16, 16), (20, 20), (24, 24), (32, 32), (40, 40), (48, 48), (64, 64), (128, 128)],
)

print(f"Generated MSIX assets in {ASSETS}")
