using System;
using Xunit;
using maplestory.io.Controllers;
using maplestory.io.Models;
using System.Threading.Tasks;

namespace IntegrationTests
{
    public class CharacterTests
    {
        [Fact]
        public async Task GetCharacter()
        {
            var character = await Character.GetCharacter("developer", "overall", "legendary");
            Assert.Equal(character.Name, "Developer");
        }

        [Fact]
        public async void GetSesuru()
        {
            var character = await Character.GetCharacter("sesuru", "overall", "legendary");
            Assert.Equal(character.Name, "Sesuru");
            // Let's be honest, I'm never going to level this character. I'm far too busy.
            Assert.Equal(character.Level, 150);
            Assert.Equal(character.Job, "Warrior");
            Assert.Equal(character.ToString(), "Sesuru the level 150 Warrior");
        }

        [Fact]
        public async void GetHakase()
        {
            var character = await Character.GetCharacter("Hakase");
            Assert.Equal(character.World, "Reboot");
            Assert.Equal(character.Job, "Thief");
        }

        [Fact]
        public async void CheckCache()
        {
            await GetCharacter();
            Assert.True(Character.IsCached("developer", "overall", "legendary"));
        }
    }
}
